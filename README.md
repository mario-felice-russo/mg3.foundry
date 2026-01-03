# Foundry Local UI

Un'interfaccia utente .NET MAUI per Microsoft Foundry Local che consente di gestire modelli AI e interagire con essi tramite chat.

## 📋 Panoramica

Questa applicazione fornisce un'interfaccia grafica per:

- **Gestione Modelli**: Scaricare, visualizzare e gestire modelli AI locali
- **Chat Interattiva**: Conversare con modelli AI con supporto multimediale
- **Monitoraggio Servizio**: Verificare lo stato del servizio Foundry Local

## 🚀 Requisiti

- .NET 9.0 SDK
- Microsoft Foundry Local installato
- Pacchetti NuGet:
  - Azure.AI.OpenAI (1.0.0-beta.17)
  - CommunityToolkit.Mvvm (8.3.2)
  - Microsoft.Maui.Controls (9.0.0)
  - Newtonsoft.Json (13.0.3)

## 🛠️ Configurazione

### File di Configurazione

Il file `appsettings.json` contiene le impostazioni principali:

```json
{
  "AppSettings": {
    "DefaultModel": "phi-3.5-mini",
    "MaxChatHistory": 100,
    "AutoRefreshInterval": 30000,
    "Theme": "Light"
  },
  "FoundrySettings": {
    "CommandTimeout": 300000,
    "MaxDownloadRetries": 3,
    "DefaultServerPort": 5272
  }
}
```

### Struttura del Progetto

```
Features/
├── Chat/              # Interfaccia e logica di chat
├── FoundryCore/       # Servizi e modelli core
├── Home/              # Pagina principale
└── ModelManagement/   # Gestione modelli
```

## 🔧 Funzionalità Principali

### 1. Servizio Foundry

- **FoundryServiceCli**: Interazione con la CLI di Foundry
- **FoundryServiceApi**: Interazione con l'API REST di Foundry Local
- Supporto per download streaming con progresso
- Gestione dinamica degli endpoint

### 2. Chat Interattiva

- Supporto multimediale (immagini e file di testo)
- Gestione automatica dei token e chunking per file grandi
- Chat streaming con completamento progressivo
- Storia della conversazione persistente

### 3. Gestione Modelli

- Visualizzazione modelli disponibili e cache
- Download e cancellazione modelli
- Informazioni dettagliate sui modelli

## 📱 Interfaccia Utente

### Pagina Principale
- Stato del servizio Foundry
- Elenco modelli cache
- Navigazione alle funzionalità principali

### Pagina Chat
- Selezione modello
- Input testuale e multimediale
- Visualizzazione cronologica dei messaggi
- Indicatori di utilizzo token

### Pagina Modelli
- Catalogo modelli disponibili
- Operazioni di download/cancellazione
- Informazioni dettagliate sui modelli

## 🔄 Flusso di Lavoro

1. **Avvio**: Verifica dello stato del servizio Foundry
2. **Selezione Modello**: Scelta del modello AI da utilizzare
3. **Chat**: Interazione con il modello tramite testo, immagini o file
4. **Gestione**: Download/cancellazione modelli secondo necessità

## 🧪 Test

Il progetto include script PowerShell per testare le funzionalità di download:

- `test_download.ps1` - Test di download di base
- `test_download_model_prop.ps1` - Test con proprietà modello
- `test_download_port.ps1` - Test con porta personalizzata
- `test_download_string.ps1` - Test con stringa semplice

## 📝 Note di Sviluppo

- Il progetto utilizza il pattern MVVM con CommunityToolkit.Mvvm
- Supporto multi-piattaforma (Windows, macOS, iOS)
- Gestione errori completa con feedback utente
- Ottimizzazione per modelli con limiti di token

## 🎯 Roadmap

- [ ] Aggiungere supporto per più provider di modelli
- [ ] Implementare salvataggio automatico delle conversazioni
- [ ] Aggiungere funzionalità di ricerca nei modelli
- [ ] Migliorare l'interfaccia utente per dispositivi mobili

## 📚 Documentazione

Consultare la documentazione dettagliata in:
- `docs/FoundryServiceApi.md` - Documentazione API
- `docs/FoundryServiceCli.md` - Documentazione CLI

## 🤝 Contributi

I contributi sono benvenuti! Si prega di:

1. Forkare il repository
2. Creare un branch per la feature (`git checkout -b feature/nome-feature`)
3. Commitare le modifiche (`git commit -m 'Aggiunta nuova feature'`)
4. Pushare il branch (`git push origin feature/nome-feature`)
5. Aprire una Pull Request

## 📜 Licenza

[Inserire informazioni sulla licenza]

---

*© 2026 Foundry Local UI - Interfaccia per Microsoft Foundry Local*