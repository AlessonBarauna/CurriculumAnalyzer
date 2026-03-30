# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Modo de Trabalho

### Postura de professor
A cada alteração feita no código, explique **o quê** foi mudado, **por quê** aquela abordagem foi escolhida, e **o que o usuário deve aprender** com ela. Prefira mostrar o raciocínio antes do código final. Se houver mais de uma solução válida, mencione as alternativas e justifique a escolha.

### Fluxo de tarefas
Ao receber um pedido de melhoria:
1. Monte um **plano de alteração** numerado antes de começar — lista de tarefas com descrição curta de cada uma.
2. Execute as tarefas uma a uma, explicando cada mudança (postura de professor acima).
3. Ao concluir **cada tarefa**, faça um commit com mensagem descritiva e dê **push** para o repositório remoto (`git push`).

### Commits
Nunca incluir `Co-Authored-By: Claude` ou qualquer referência ao assistente nas mensagens de commit.

### Plano de alteração
Sempre que o usuário pedir para "seguir com melhorias", "continuar", "próxima feature" ou similar, **primeiro apresente o plano** (lista numerada) e aguarde confirmação antes de executar. Só pule essa etapa se o usuário disser explicitamente para começar sem plano.

## Project Overview

**CurriculumAnalyzer** is a full-stack AI-powered resume analysis app. Users upload a CV (PDF/DOCX/TXT), provide career context, and receive structured feedback from the Groq API (llama-3.3-70b-versatile). Output includes section scores, strengths/weaknesses, opportunities, action plan, job recommendations, and BRL salary estimates.

- **Backend**: .NET 10 ASP.NET Core — `CurriculumAnalyzer.API/`
- **Frontend**: Angular 21 standalone components — `curriculum-analyzer-frontend/`
- **Database**: SQLite (EF Core) — auto-migrated on startup
- **AI**: Groq API with a Portuguese-language prompt
- **Deployment**: Railway (backend) / Vercel (frontend)

## Running

```bash
# Backend — https://localhost:5001 / http://localhost:5080
cd CurriculumAnalyzer.API
dotnet run
# Swagger: http://localhost:5080/swagger/ui

# Frontend — http://localhost:4200
cd curriculum-analyzer-frontend
npm install
ng serve
```

The Groq API key must be set in `CurriculumAnalyzer.API/appsettings.Development.json`:
```json
{ "Groq": { "ApiKey": "<your-key>" } }
```

## Build & Test

```bash
# Backend
cd CurriculumAnalyzer.API
dotnet build

# Frontend
cd curriculum-analyzer-frontend
ng build        # production build
ng test         # Vitest
```

## Database Migrations

```bash
cd CurriculumAnalyzer.API
dotnet ef migrations add <Name>
dotnet ef database update
```

SQLite file: `CurriculumAnalyzer.API/curriculum_analyzer.db` — migrations also run automatically on `app.Run()` via `db.Database.Migrate()` in `Program.cs`.

---

## Architecture

### Request Flow

1. `UploadComponent` (reactive form) POSTs multipart to `POST /api/curriculum/upload-and-analyze`
2. `CurriculumController` validates file (PDF/DOCX/TXT, max 5 MB) and deserializes `UserContextModel` from the `context` form field (JSON string)
3. `FileProcessingService` extracts raw text (itext7 → PDF, OpenXML → DOCX, direct read → TXT)
4. SHA-256 hash of `rawText|experienceLevel|specialization|marketObjective|currentLocation` is computed — cache hit returns existing `analysisId` immediately
5. `CurriculumAnalysisService` calls `GrokAiService` → maps response → stores `AnalysisEntity` → returns `AnalysisResponseDto`
6. Frontend navigates to `/analysis/:id`

### Backend Service Conventions

Services are **concrete classes with no interfaces** — inject and register them directly:

```csharp
// Program.cs — standard pattern
builder.Services.AddScoped<FileProcessingService>();
builder.Services.AddScoped<CurriculumAnalysisService>();

// GrokAiService needs IHttpClientFactory — registered with a factory lambda:
builder.Services.AddScoped<GrokAiService>(sp =>
    new GrokAiService(
        sp.GetRequiredService<IHttpClientFactory>().CreateClient(),
        sp.GetRequiredService<IConfiguration>(),
        sp.GetRequiredService<ILogger<GrokAiService>>()));
```

### JSON Storage Pattern

`AnalysisEntity` stores AI result arrays/objects as raw JSON strings (`SectionsJson`, `StrengthsJson`, etc.). When reading from the DB, always use the `TryDeserialize<T>` helper defined in `AnalysisController`:

```csharp
private static T? TryDeserialize<T>(string json, JsonSerializerOptions options)
{
    if (string.IsNullOrWhiteSpace(json)) return default;
    try { return JsonSerializer.Deserialize<T>(json, options); }
    catch { return default; }
}
```

Always use `PropertyNameCaseInsensitive = true` when deserializing from stored JSON or Groq responses.

### AI Prompt & Response

The full prompt is in `GrokAiService.BuildAnalysisPrompt()`. It is Portuguese, instructs the model to return **only valid JSON** (no markdown), and provides the exact JSON schema inline. The parser in `ParseGrokResponse()` handles the common case where the model wraps the JSON in triple-backtick markdown anyway — it finds the first `{` and last `}`.

To change what the AI returns, update both:
1. The prompt template in `BuildAnalysisPrompt()`
2. The corresponding DTO properties in `Models/Dto/`
3. The entity JSON columns in `AnalysisEntity` + a new migration

### Angular Component Conventions

All components are **standalone** (`standalone: true`). State is managed with plain class fields (`loading`, `errorMessage`) — no signals, no NgRx. `ChangeDetectorRef.detectChanges()` is called manually after async observable callbacks in `AnalysisReportComponent`.

```typescript
// Standard component scaffold
@Component({
  selector: 'app-xyz',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './xyz.component.html',
  styleUrls: ['./xyz.component.scss']
})
export class XyzComponent implements OnInit { ... }
```

Services are `providedIn: 'root'` singletons. All routes are **eagerly loaded** — no lazy loading.

### Error Handling

Backend returns error objects as `{ error: "message" }`. Frontend consumes them as:
```typescript
error: (err) => { this.errorMessage = err?.error?.error || 'Fallback message'; }
```

### API Endpoints

| Method | Path | Description |
|--------|------|-------------|
| POST | `/api/curriculum/upload-and-analyze` | Upload CV + context → `{ analysisId }` |
| GET | `/api/analysis` | History list, sorted by date desc |
| GET | `/api/analysis/{id}` | Full analysis DTO |
| GET | `/api/analysis/compare?id1=x&id2=y` | Compare two analyses (partial DTO — only Sections, Strengths, Weaknesses) |

### Key Configuration

| Setting | Where |
|---------|-------|
| Groq API key (dev) | `appsettings.Development.json` → `Groq:ApiKey` |
| Groq API key (prod) | Railway env var `Groq:ApiKey` |
| CORS origins | `appsettings.json` → `Cors:AllowedOrigins` (comma-separated) |
| Frontend API URL (dev) | `src/environments/environment.ts` |
| Frontend API URL (prod) | `src/environments/environment.prod.ts` |

`ApiConfigService` resolves the URL at runtime by checking `window.location.hostname` — no build-time token replacement needed.

### Gotchas

- `GrokApiResponse`, `GrokChoice`, and `GrokMessage` are defined at the bottom of `GrokAiService.cs`, not in a models folder.
- `CurriculumAnalysisService.AnalyzeCurriculumAsync` mutates the DTO returned from `GrokAiService` to backfill `Id`, `CurriculumId`, and `AnalysisDate` after the entity is saved.
- The `compare` endpoint intentionally maps only `Sections`, `Strengths`, and `Weaknesses` — `Opportunities`, `ActionPlan`, and `JobRecommendations` are omitted in the comparison view.
- Salary currency is hardcoded as `"BRL"` everywhere — in the prompt, in `SalaryRangeDto`, and in the `EstimatedSalaryRange` factory.
- File validation is duplicated on both frontend (`UploadComponent`) and backend (`CurriculumController`) — keep them in sync when changing allowed types or size limits.
