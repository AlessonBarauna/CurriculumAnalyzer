import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { CurriculumService } from '../../shared/services/curriculum.service';

@Component({
  selector: 'app-upload',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule],
  templateUrl: './upload.component.html',
  styleUrls: ['./upload.component.scss']
})
export class UploadComponent {
  uploadForm: FormGroup;
  loading = false;
  selectedFile: File | null = null;
  errorMessage = '';

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
    private router: Router
  ) {
    this.uploadForm = this.fb.group({
      experienceLevel: ['mid-level', Validators.required],
      specialization: ['backend', Validators.required],
      marketObjective: ['', Validators.required],
      targetSalary: [''],
      currentLocation: ['Brazil', Validators.required]
    });
  }

  onFileSelected(event: Event): void {
    const target = event.target as HTMLInputElement;
    const files = target.files;
    this.errorMessage = '';

    if (!files || files.length === 0) return;

    const file = files[0];
    const validTypes = [
      'application/pdf',
      'application/vnd.openxmlformats-officedocument.wordprocessingml.document',
      'text/plain'
    ];

    if (!validTypes.includes(file.type)) {
      this.errorMessage = 'Arquivo inválido. Use PDF, DOCX ou TXT.';
      return;
    }

    if (file.size > 5 * 1024 * 1024) {
      this.errorMessage = 'Arquivo muito grande (máximo 5MB).';
      return;
    }

    this.selectedFile = file;
  }

  async onSubmit(): Promise<void> {
    if (!this.uploadForm.valid || !this.selectedFile) {
      this.errorMessage = 'Preencha todos os campos obrigatórios e selecione um arquivo.';
      return;
    }

    this.loading = true;
    this.errorMessage = '';

    const formData = new FormData();
    formData.append('file', this.selectedFile);
    formData.append('context', JSON.stringify(this.uploadForm.value));

    this.curriculumService.uploadAndAnalyze(formData).subscribe({
      next: (result) => {
        this.router.navigate(['/analysis', result.analysisId]);
      },
      error: (err) => {
        this.errorMessage = err?.error?.error || 'Erro ao processar arquivo. Tente novamente.';
        this.loading = false;
      }
    });
  }
}
