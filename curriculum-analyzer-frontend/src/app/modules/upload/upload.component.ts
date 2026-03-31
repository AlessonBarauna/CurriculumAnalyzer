import { Component, signal, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { interval, Subscription } from 'rxjs';
import { CurriculumService } from '../../shared/services/curriculum.service';
import { AuthService } from '../../shared/services/auth.service';

interface ProgressStep {
  label: string;
  icon: string;
  durationMs: number;
}

const STEPS: ProgressStep[] = [
  { label: 'Enviando arquivo...', icon: '📤', durationMs: 1500 },
  { label: 'Extraindo texto do currículo...', icon: '📄', durationMs: 2500 },
  { label: 'Analisando com inteligência artificial...', icon: '🤖', durationMs: 99999 },
  { label: 'Salvando sua análise...', icon: '💾', durationMs: 0 }
];

@Component({
  selector: 'app-upload',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule],
  templateUrl: './upload.component.html',
  styleUrls: ['./upload.component.scss']
})
export class UploadComponent implements OnDestroy {
  uploadForm: FormGroup;
  loading = signal(false);
  selectedFile = signal<File | null>(null);
  errorMessage = signal('');
  currentStep = signal(0);
  readonly steps = STEPS;

  private stepTimer?: Subscription;

  levelOptions = [
    { value: 'junior', label: 'Junior (0-2 anos)' },
    { value: 'mid-level', label: 'Mid-Level (2-5 anos)' },
    { value: 'senior', label: 'Sênior (5+ anos)' }
  ];

  specializationOptions = [
    { value: 'backend', label: 'Backend Developer' },
    { value: 'frontend', label: 'Frontend Developer' },
    { value: 'fullstack', label: 'FullStack Developer' },
    { value: 'devops', label: 'DevOps Engineer' },
    { value: 'data-engineer', label: 'Data Engineer' },
    { value: 'qa', label: 'QA / Test Engineer' },
    { value: 'security', label: 'Security Engineer' }
  ];

  constructor(
    private fb: FormBuilder,
    private curriculumService: CurriculumService,
    private router: Router,
    public authService: AuthService
  ) {
    this.uploadForm = this.fb.group({
      experienceLevel: ['mid-level', Validators.required],
      specialization: ['backend', Validators.required],
      marketObjective: ['', Validators.required],
      targetSalary: [''],
      currentLocation: ['Brazil', Validators.required]
    });
  }

  ngOnDestroy(): void {
    this.stepTimer?.unsubscribe();
  }

  onFileSelected(event: Event): void {
    const target = event.target as HTMLInputElement;
    const files = target.files;
    this.errorMessage.set('');

    if (!files || files.length === 0) return;

    const file = files[0];
    const validTypes = [
      'application/pdf',
      'application/vnd.openxmlformats-officedocument.wordprocessingml.document',
      'text/plain'
    ];

    if (!validTypes.includes(file.type)) {
      this.errorMessage.set('Arquivo inválido. Use PDF, DOCX ou TXT.');
      return;
    }

    if (file.size > 5 * 1024 * 1024) {
      this.errorMessage.set('Arquivo muito grande (máximo 5MB).');
      return;
    }

    this.selectedFile.set(file);
  }

  onSubmit(): void {
    if (!this.uploadForm.valid || !this.selectedFile()) {
      this.errorMessage.set('Preencha todos os campos obrigatórios e selecione um arquivo.');
      return;
    }

    this.loading.set(true);
    this.currentStep.set(0);
    this.errorMessage.set('');
    this.startStepTimer();

    const formData = new FormData();
    formData.append('file', this.selectedFile()!);
    formData.append('context', JSON.stringify(this.uploadForm.value));

    this.curriculumService.uploadAndAnalyze(formData).subscribe({
      next: (result) => {
        this.stopStepTimer();
        this.currentStep.set(STEPS.length - 1);
        this.loading.set(false);
        this.router.navigate(['/analysis', result.analysisId]).then(navigated => {
          if (!navigated) {
            this.errorMessage.set('Análise concluída! Acesse o histórico para visualizá-la.');
            this.currentStep.set(0);
          }
        });
      },
      error: (err) => {
        this.stopStepTimer();
        this.errorMessage.set(err?.error?.error || 'Erro ao processar arquivo. Tente novamente.');
        this.loading.set(false);
        this.currentStep.set(0);
      }
    });
  }

  private startStepTimer(): void {
    let elapsed = 0;
    this.stepTimer = interval(200).subscribe(() => {
      elapsed += 200;
      const step = this.currentStep();
      if (step < STEPS.length - 2 && elapsed >= STEPS[step].durationMs) {
        this.currentStep.set(step + 1);
        elapsed = 0;
      }
    });
  }

  private stopStepTimer(): void {
    this.stepTimer?.unsubscribe();
  }

  goToHistory(): void {
    this.router.navigate(['/history']);
  }

  logout(): void {
    this.authService.logout();
    this.router.navigate(['/login']);
  }
}
