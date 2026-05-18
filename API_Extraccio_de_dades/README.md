# 📄 Projecte-Final: Sistema Intel·ligent de Gestió i OCR

## 🌟 Resum
**Projecte-Final** és una solució integral dissenyada per automatitzar la ingesta i gestió de documents de negoci (Ordres de Compra) mitjançant Intel·ligència Artificial. Combina un potent **Backend en .NET** amb una **Interfície Desktop en WPF**, permetent processar documents en paral·lel, validar extraccions mitjançant IA (GPT-4o) i persistir la informació de forma estructurada en una base de dades SQL.

El sistema està construït seguint els principis de **Clean Architecture**, assegurant una clara separació entre la lògica de domini, aplicació i infraestructura.

---

## 🚀 Característiques Principals

### 🖥️ Frontend (Desktop App)
- **Interfície Moderna**: Disseny amb tema fosc, micro-animacions i UX optimitzada.
- **Gestió d'Arxius**: Visualització en temps real de documents pendents i processats.
- **Processament en Paral·lel**: Capacitat per enviar múltiples documents simultàniament per al seu anàlisi per IA.
- **Previsualització Intel·ligent**: Visualització del JSON extret abans de confirmar el desament a la base de dades.
- **Control de Flux**: Sistema de sincronització i estats de procés (Pendent, Processant, Completat).

### ⚙️ Backend (API & AI Service)
- **Integració amb OpenAI**: Ús de models avançats (GPT-4o-mini) per convertir PDFs no estructurats en dades JSON precises.
- **Arquitectura Neta**: Estructura multicapa (Domain, Application, Business, Infrastructure, ServiceOCR).
- **Gestió d'Emmagatzematge**: Gestió automatitzada de carpetes `ToProcess`, `Processed` i `JSON`.
- **Validació Robusta**: Implementació de validadors a nivell d'infraestructura i domini per assegurar la integritat de les dades.
- **Endpoints d'Ingesta**: API optimitzada per rebre i mapejar dades OCR a entitats de negoci (Clients, Proveïdors, Ordres).

---

## 🛠️ Stack Tecnològic
- **Frontend**: WPF (C# / XAML) amb CommunityToolkit.Mvvm.
- **Backend Framework**: .NET 8/9 (Minimal APIs).
- **Intel·ligència Artificial**: OpenAI API (GPT-4o / GPT-4o-mini).
- **Base de Dades**: Microsoft SQL Server.
- **Serialització**: System.Text.Json / Newtonsoft.Json.

---

## 📂 Estructura del Projecte
```bash
Backend/
├── Application/        # Endpoints de la API i definicions de DTO
├── Business/           # Lògica central i serveis de negoci
├── Domain/             # Entitats i regles de negoci
├── Infrastructure/     # Accés a dades i validadors
├── ServiceOCR/         # Integració amb OpenAI i gestió d'arxius
├── SQL/                # Scripts de base de dades
└── Storage/            # Repositori local d'arxius (ToProcess, Processed, JSON)

Frontend/
├── Models/             # Models de dades compartits i UI
├── ViewModels/         # Lògica de la interfície (MVVM)
├── Services/           # Comunicació amb la API Backend
├── Styles/             # Definicions de disseny i temes (ModernStyles.xaml)
└── MainWindow.xaml     # Layout principal de la aplicació
```

---

## 🚦 Primers Passos

### Requisits Previs
- [.NET SDK](https://dotnet.microsoft.com/download)
- [SQL Server](https://www.microsoft.com/sql-server)
- **API Key d'OpenAI** (configurada al backend)

### Configuració del Backend
1. Actualitza la cadena de connexió i la teva API Key a `Backend/appsettings.json`:
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=EL_TEU_SERVIDOR;Database=Projecte;User Id=sa;..."
  },
  "OpenAI": {
    "ApiKey": "LA_TEVA_OPENAI_API_KEY"
  }
}
```
2. Executa els scripts SQL al teu servidor per crear les taules necessàries.

### Execució
1. **Iniciar el Backend**:
   ```bash
   cd Backend
   dotnet run
   ```
2. **Iniciar el Frontend**:
   ```bash
   cd Frontend
   dotnet run
   ```

---

## 🔌 Flux de Treball
1. Col·loca els documents PDF a la carpeta `Backend/Storage/ToProcess`.
2. Prem "Sincronitzar" a la App per veure els arxius disponibles.
3. Selecciona els arxius i prem "PROCESSAR". La IA extreure les dades.
4. Una vegada extretes, pots fer "Veure JSON" o "💾 Desar".
5. Al desar, les dades s'insereixen a SQL Server i l'arxiu original es mou automàticament a `Processed`.

---

## 👨‍💻 Autor
Projecte final enfocat en l'automatització de processos industrials mitjançant IA i arquitectures distribuïdes modernes.
