# Memòria general — Projecte Final DAM

> **Document 1 de 4** · Memòria acadèmica i context del projecte  
> Documentació tècnica: [API OCR](../API_Extraccio/Documentacio_Tecnica.md) · [API ERP](../API_ERP/Documentacio_Tecnica.md) · [Frontend](../Frontend/Documentacio_Tecnica.md)

---

## Índex

1. [Portada i dades del projecte](#1-portada-i-dades-del-projecte)
2. [Resum](#2-resum)
3. [Introducció](#3-introducció)
4. [Empresa col·laboradora](#4-empresa-col·laboradora)
5. [Objectius](#5-objectius)
6. [Plantejament de la solució](#6-plantejament-de-la-solució)
7. [Requisits](#7-requisits)
8. [Proves i evidències](#8-proves-i-evidències)
9. [Conclusions](#9-conclusions)
10. [Bibliografia](#10-bibliografia)

---

## 1. Portada i dades del projecte

| Camp | Valor |
|------|-------|
| Títol del projecte | Automatització d'ordres de compra mitjançant OCR (IA) i integració amb ERP |
| Alumne/a | Marc Morera Prat |
| Centre educatiu | Institut Bosc de la Coma |
| Curs acadèmic | 2024–2026 |
| Cicle | DAM — Desenvolupament d'Aplicacions Multiplataforma |
| Empresa col·laboradora | Bossauto Innova S.A.U. |

---

## 2. Resum

Bossauto Innova S.A.U. dedica una part important del temps del personal a **traspassar manualment** les ordres de compra rebudes en PDF cap al seu sistema ERP. L'objectiu principal del projecte és **agilitzar aquest procés** i reduir la càrrega de treball i els errors de transcripció.

Després de contrastar les necessitats amb l'empresa, s'ha desenvolupat una **aplicació d'escriptori** independent que processa els PDF mitjançant un **servei OCR (IA)**, permet revisar les dades extretes i les emmagatzema a **SQL Server**. El traspàs cap a l'ERP s'ha provat amb **Odoo 17** (entorn de proves pactat amb l'empresa); en **producció**, Bossauto treballa amb **Microsoft Dynamics NAV (Navision)**. El resultat és un flux semi-automatitzat, controlat per l'usuari, alineat amb un cas d'ús real.

---

## 3. Introducció

### 3.1 Context

Aquest projecte final s'ha dut a terme en col·laboració amb **Bossauto Innova S.A.U.**, amb l'objectiu de trobar una solució de programari que redueixi el temps dedicat a introduir comandes de compra al sistema de gestió.

L'empresa opera al sector de l'**automoció i els recanvis** i, a més, realitza **termoformat de plàstic** per a envàsos de pintura. Les ordres de compra dels clients arriben habitualment en **PDF**; abans del projecte, el personal les revisava document per document i havia de **introduir-les manualment** (teclear-les) a l'ERP, un procés repetitiu i susceptible d'errors.

### 3.2 Problemàtica

Les ordres de compra arriben sovint en **PDF** o formats no estructurats. Introduir-les manualment a un ERP consumeix temps, genera errors i dificulta el seguiment de cada ordre.

El projecte aborda aquesta problemàtica amb una aplicació que:

1. Llegeix el PDF mitjançant un **servei OCR (IA)**.
2. Permet **revisar** les dades extretes.
3. Les guarda a **SQL Server**.
4. Les traspassa a l'ERP de proves (**Odoo**) com a pressupost de venda (en producció l'empresa utilitza **Navision**).

### 3.3 Objectiu del document

Aquesta memòria descriu el context, el plantejament i els resultats del projecte. La **documentació tècnica** (endpoints, JSON, frontend) es troba als altres tres fitxers enllaçats a l'inici.

---

## 4. Empresa col·laboradora

- **Nom de l'empresa:** Bossauto Innova S.A.U.
- **Sector d'activitat:** Automoció i recanvis; termoformat de plàstic per a envàsos de pintura.
- **Necessitat concreta** que va motivar el projecte: reduir el temps i els errors en el pas de les ordres de compra en PDF cap a l'ERP.
- **Com s'ha treballat amb l'empresa** (reunions, feedback, validacions): La coordinació amb Bossauto Innova s'ha fet de forma **presencial i telemàtica**, ajustant el desenvolupament segons les necessitats detectades. S'ha utilitzat **WhatsApp** per al seguiment quotidià, s'ha realitzat una **demostració** del funcionament de l'aplicació i, a l'inici del projecte, una visita presencial per conèixer el procés real d'introducció de comandes.
- **ERP o sistemes:**
  - **Producció (empresa):** **Microsoft Dynamics NAV (Navision)** — sistema on s'introdueixen actualment les comandes de forma manual.
  - **Proves del projecte:** **Odoo 17** (Docker), acordat amb l'empresa per poder desenvolupar i demostrar el traspàs sense impactar el Navision de producció. L'API ERP crea pressupostos de venda (`sale.order`) en aquest entorn.

---

## 5. Objectius

### Objectius generals

- Automatitzar la lectura d'ordres de compra en PDF.
- Reduir la introducció manual de dades.
- Permetre revisió humana abans de guardar i abans del traspàs ERP.
- Demostrar el traspàs cap a un ERP (Odoo en proves; equivalent funcional al flux cap a Navision en producció).
- Evitar duplicats mitjançant el camp `estado` de les ordres.

---

## 6. Plantejament de la solució

L'arquitectura, el flux de 14 passos, les tecnologies i l'abast del sistema estan detallats al document:

**[03_Plantejament.md](03_Plantejament.md)**

**Mapa de processos** (flux d'alt nivell):

![Mapa de processos d'extracció de dades](Mapa_de_proccesos.png)

El **model de dades** (taules `clientes`, `ordenes`, `lineas_orden`) i el flux detallat de 14 passos es descriuen al document de plantejament enllaçat més amunt.

---

## 7. Requisits

### Requisits funcionals

| Codi | Descripció |
|------|------------|
| RF01 | Mostrar PDFs pendents de processar |
| RF02 | Seleccionar un o diversos documents |
| RF03 | Enviar documents a l'API OCR |
| RF04 | Rebre JSON amb dades extretes (OCR IA) |
| RF05 | Revisar dades abans de guardar |
| RF06 | Guardar client, ordre i línies a SQL Server |
| RF07 | Ordres amb `estado = 0` (pendents Odoo) |
| RF08 | Pestanya Odoo: només ordres no traspassades |
| RF09 | Crear pressupost a Odoo des de l'aplicació |
| RF10 | Actualitzar `estado = 1` després del traspàs |
| RF11 | Evitar traspassar la mateixa ordre dues vegades |
| RF12 | Descartar o moure documents erronis |

### Requisits no funcionals

| Codi | Descripció |
|------|------------|
| RNF01 | Interfície clara i usable |
| RNF02 | Separació UI / API OCR / API ERP |
| RNF03 | Missatges d'error comprensibles |
| RNF04 | Integritat referencial a la BD |
| RNF05 | Execució en entorn local |
| RNF06 | Connexió ERP via API dedicada |
| RNF07 | Configuració via `appsettings.json` |
| RNF08 | Possibilitat d'integrar altres ERPs en el futur |

---

## 8. Proves i evidències

### 8.1 Taula resum de proves

| # | Prova | Resultat esperat | Resultat obtingut | OK? |
|---|-------|------------------|-------------------|-----|
| 1 | Llistar PDFs pendents | Es mostren fitxers a ToProcess | Correcte | Sí |
| 2 | Processar PDF | JSON amb dades extretes (OCR IA) | Correcte | Sí |
| 3 | Guardar ordre | Missatge d'èxit, registre a SQL | Correcte | Sí |
| 4 | Llistar ordres Odoo | Només `estado = 0` | Correcte | Sí |
| 5 | Crear pressupost | `sale.order` a Odoo, `estado = 1` | Correcte | Sí |

### 8.2 Documentació tècnica de referència

- [API OCR](../API_Extraccio/Documentacio_Tecnica.md) — endpoints i JSON
- [API ERP](../API_ERP/Documentacio_Tecnica.md) — integració Odoo
- [Frontend](../Frontend/Documentacio_Tecnica.md) — flux d'usuari WPF

---

## 9. Conclusions

Al final del projecte puc dir que he aconseguit el que em proposava: una aplicació que agafa un PDF, en treu les dades amb **OCR (IA)**, deixa revisar-les, les guarda a **SQL Server** i les pot enviar a **Odoo** com a pressupost. Les proves de l'apartat 8 han anat bé i això em dóna confiança que el flux té sentit. A l'empresa, en el dia a dia, el que realment els costa és passar les comandes al **Navision**; amb Odoo he pogut provar la mateixa idea sense tocar el sistema de producció, tal com havia quedat amb **Bossauto Innova S.A.U.**

Per a ells, crec que el valor està clar: menys hores copiant dades d'un PDF i menys risc d'equivocar-se en una línia o en un import. Que l'usuari pugui revisar abans de guardar també és important perquè no perden el control del que entra al sistema. La visita al principi, les converses per WhatsApp i la demo del final han ajudat molt a no desenvolupar «a cegues».

A nivell personal, he après molt de tècnic — APIs, WPF, base de dades, connexió amb un ERP — i em queda el que he viscut treballant amb una empresa de veritat. No és el mateix que un exercici de classe: tenen molta feina, les respostes triguen i cal anar adaptant el que fas. Si ho tornés a fer, començaria abans amb ells i tancaria un calendari de reunions des del primer mes; això crec que m'hauria estalviat presses d'última hora.

En conjunt, m'ha semblat un projecte molt interessant perquè per primer cop he vist com el que programo serveix per resoldre un problema real. Em quedo amb la sensació que tot el que hem estudiat al **DAM** tenia un sentit: no era només codi, era arribar a alguna cosa útil per a algú.

---

## 10. Bibliografia

- Microsoft (2024). *Documentació de .NET, ASP.NET Core i WPF.* https://learn.microsoft.com/dotnet/
- Odoo S.A. *Documentació Odoo 17 — API externa i JSON-RPC.* https://www.odoo.com/documentation/17.0/
- Microsoft (2024). *Documentació de SQL Server.* https://learn.microsoft.com/sql/
- Materials i apunts del cicle formatiu **DAM** (Desenvolupament d'Aplicacions Multiplataforma), Institut Bosc de la Coma.
