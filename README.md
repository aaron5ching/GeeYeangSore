# 居研所後端系統（ASP.NET Core MVC）

本專案為租屋媒合平台「居研所」的後端系統，採用 ASP.NET Core MVC 架構開發，提供完整的 API 與後台管理功能。系統支援會員註冊登入、房源刊登與審核、聊天室即時通訊、金流付款、數據視覺化等核心功能，並整合 SignalR、EF Core 與 ECPay 等技術。

前端則採用 Vue 3 搭配 Pinia 與 Vue Router 開發，透過 Axios 串接本專案所提供的 RESTful API，實作前台使用者介面與互動流程，完整實現房客與房東雙角色的操作體驗。


---


### 1. Admin Area（管理後台）

#### 技術架構
- **MVC 架構**：每個功能模組皆有獨立 Controller、View、ViewModel，實現高內聚低耦合。
- **多層目錄結構**：依功能細分 Controllers 子目錄（如房源、會員、金流、數據分析等），利於大型後台維護。
- **Razor View**：後台頁面以 Razor View 呈現，支援動態資料綁定與互動。
- **ViewModel 對應**：每個頁面/功能皆有專屬 ViewModel，確保資料交換安全與明確。
- **權限控管**：僅管理員可進入，並於 Controller 層進行權限驗證。
- **分頁、搜尋、篩選**：多數管理頁面支援分頁、條件搜尋與篩選，提升管理效率。
- **資料視覺化**：數據分析、金流報表等功能整合圖表（如 Chart.js）進行視覺化展示。

#### 主要功能模組

| 模組 | 說明 |
|------|------|
| 房源審核（PropertyCheck） | 管理員審核新上架房源，檢查資料、圖片、合規性，並可駁回或通過。 |
| 會員管理（UserManagement） | 查詢、編輯、停權房東/房客帳號，管理管理員權限。 |
| 房源管理（Property） | 編輯、下架、刪除房源，管理房源詳細資料。 |
| 廣告管理（PropertyAD） | 管理房東購買的廣告方案、刊登狀態與審核。 |
| 最新消息/公告（News） | 發布、編輯、刪除平台公告與最新消息。 |
| 平台導覽/說明（Guide） | 管理新手教學、平台使用說明內容。 |
| 關於我們（About） | 編輯平台簡介、聯絡資訊等靜態頁面。 |
| 審核紀錄（Audit） | 查詢房源、會員等各類審核紀錄。 |
| 訊息管理（Messages） | 管理站內私訊、群組訊息，查詢訊息紀錄。 |
| 金流管理（FinanceAdmin） | 查詢金流交易、退款、財務報表，管理付款狀態。 |
| 數據分析（DataAnalysis） | 平台各類數據統計、趨勢分析、圖表報表。 |
| 聯絡客服（Contact） | 處理用戶意見回饋、客服聯絡紀錄。 |
| 首頁儀表板（Home） | 顯示平台總覽、即時數據、待辦事項等。 |

### 2. Identity Area（身分驗證區）

#### 技術架構
- **ASP.NET Core Identity**：整合官方身分驗證框架，支援會員註冊、登入、密碼重設、信箱驗證等。
- **Razor Pages**：採用 Razor Pages 實作，簡化驗證流程頁面開發。
- **安全性設計**：密碼雜湊、信箱驗證、Token 驗證等多重安全機制。
- **與 Session/Claims 整合**：登入後將身分資訊寫入 Session 或 Claims，供全站權限控管使用。

#### 主要功能模組

| 模組 | 說明 |
|------|------|
| 註冊/登入 | 會員註冊、登入、登出。 |
| 信箱驗證 | 註冊後發送驗證信，點擊連結啟用帳號。 |
| 密碼重設 | 忘記密碼申請、信箱驗證、重設新密碼。 |
| 個人資料管理 | 編輯個人基本資料、變更密碼。 |

---


- **ASP.NET Core MVC**：採用分層架構，Controller 負責 API 路由與業務邏輯協調，Models/DTO/ViewModels 處理資料結構與前後端交換。
- **Entity Framework Core**：以 Code First 方式設計資料庫，集中於 GeeYeangSoreContext，支援資料遷移與關聯查詢。
- **RESTful API 設計**：所有功能皆以 RESTful API 提供，前端（Vue 3）透過 Axios 串接。
- **SQL Server 資料庫**：結構化儲存會員、房源、聊天、金流、檢舉等多元資料。
- **Session 驗證機制**：以 Session 管理三種身分（房東、房客、管理員），並於 API 層進行權限控管。
- **SignalR 即時通訊**：ChatHub 實作聊天室即時訊息、圖片上傳、封鎖用戶等功能。
- **綠界金流（ECPay）串接**：金流模組整合 ECPay API，支援付款、驗證、交易紀錄與回調處理。
- **Swagger API 文件**：自動生成 API 文件，方便開發與測試。
- **多層資料傳輸物件（DTO/ViewModel）**：明確區分資料庫模型、API 輸入/輸出結構，提升維護性與安全性。
- **模組化目錄結構**：APIControllers 依功能分目錄（如 Chat、Commerce、Notice、Landlord 等），利於團隊協作與維護。
- **Areas 分區管理**：支援管理後台、會員專區等多區域功能擴展。
- **設定檔集中管理**：如 SMTP、金流等第三方服務設定集中於 Settings 目錄。

---

## 📦 系統模組與功能

| 模組 | 說明 |
|------|------|
| 🔐 登入與權限驗證 | 管理三種身分（房東、房客、管理員），使用 Session 儲存登入資訊，區分後台頁面權限 |
| 💬 聊天管理 | SignalR 即時通訊，含訊息紀錄查詢、圖片上傳預覽、封鎖用戶功能 |
| 🚨 檢舉管理 | 管理員可查看檢舉紀錄、標記處理狀態、依類型分類與統計 |
| 💳 金流模組 | 串接綠界金流 API，支援房東購買方案付款、交易紀錄保存、回傳驗證與查詢 |
| 📈 金流統計 | 管理後台可查看金流報表、付款狀態統計、付款方式比例圖（Chart.js） |
| 🏠 房源與會員資料管理 | 管理員可修改房源資訊、停權房東／房客帳號、搜尋篩選 |

---

## API 功能模組

APIControllers 依據業務功能分目錄，對應 RESTful API，前端（Vue 3）透過 Axios 串接。

| 模組 | 說明 |
|------|------|
| Auth | 註冊、登入、信箱驗證、忘記密碼等認證相關 API。 |
| Session | Session 驗證、權限控管、身分切換等。 |
| UserHome | 會員個人頁資料查詢與編輯。 |
| Landlord | 房東專區：刊登/管理房源、購買廣告、查詢交易紀錄。 |
| PropertySearch | 房源搜尋、篩選、詳情查詢。 |
| Favorite | 收藏房源、管理收藏清單。 |
| Chat | 即時聊天、訊息紀錄、圖片上傳、聊天室封鎖。 |
| Notice | 檢舉紀錄查詢、分類、狀態標記。 |
| Commerce | 金流付款、交易紀錄、ECPay 串接、回調驗證、退款。 |
| Contact | 聯絡客服、意見回饋。 |
| Guide | 新手導覽、平台教學。 |
| About | 關於我們、平台資訊。 |
| test | 測試用 API，開發階段功能驗證。 |

---

## API 技術架構

- **RESTful 設計**：所有 API 皆遵循 RESTful 標準，路由清晰、資源導向，支援 GET/POST/PUT/DELETE 等動詞。
- **分層目錄結構**：APIControllers 依功能分目錄（如 Chat、Commerce、Landlord 等），每個模組獨立維護，利於團隊協作與維護。
- **共用基底控制器**：所有前台 API 繼承 BaseController，統一注入資料庫、Session 驗證、權限檢查、黑名單判斷等共用邏輯。
- **Session 驗證與權限控管**：API 內建 Session 驗證，根據身分（房東/房客/管理員）與黑名單狀態自動攔截未授權請求。
- **DTO/ViewModel 輸入輸出**：API 嚴格區分資料庫模型與對外資料結構，提升安全性與維護性。
- **例外處理與回應格式**：統一回傳 JSON 格式，包含 success 狀態、訊息、資料內容，便於前端處理。
- **即時通訊支援**：聊天模組結合 SignalR，支援即時訊息推播與圖片上傳。
- **金流安全設計**：金流 API 整合 ECPay，具備交易驗證、回調處理、資料加密等安全機制。
- **API 文件自動化**：整合 Swagger，自動生成 API 文件，方便開發與測試。
- **模組化擴充**：每個功能模組可獨立擴充 Controller、DTO、Service，支援大型專案需求。

### API 功能模組

| 模組 | 主要 API 功能 |
|------|--------------|
| Auth | 註冊、登入、信箱驗證、忘記密碼、重設密碼、Token 驗證。 |
| Session | Session 驗證、身分切換、權限查詢。 |
| UserHome | 會員個人資料查詢、編輯、歷史紀錄查詢。 |
| Landlord | 房東申請、房源刊登/管理、廣告購買、個人交易紀錄查詢。 |
| PropertySearch | 房源搜尋、條件篩選、房源詳情查詢、推薦房源。 |
| Favorite | 收藏房源、取消收藏、收藏清單查詢。 |
| Chat | 即時聊天、訊息紀錄查詢、聊天室封鎖、圖片訊息上傳。 |
| Notice | 檢舉紀錄查詢、檢舉分類、狀態標記、檢舉統計。 |
| Commerce | 金流付款、ECPay 串接、交易紀錄查詢、回調驗證、退款申請。 |
| Contact | 聯絡客服、意見回饋、客服紀錄查詢。 |
| Guide | 新手導覽、平台教學內容查詢。 |
| About | 關於我們、平台資訊查詢。 |
| test | 測試用 API，驗證 Session、權限、資料存取等。 |

---


以下功能皆由我主導開發，前端串接後端 RESTful API 並實作完整互動流程：

**前台 API（APIControllers）**
- Commerce 模組（`APIControllers/Commerce`）：金流串接（ECPay）、付款流程、交易紀錄、Callback 寫入、付款查詢驗證
- 登入與驗證（`APIControllers/AuthController.cs`）：會員登入、註冊、驗證流程
- 共用基底控制器（`APIControllers/BaseController.cs`）：Session 驗證、權限控管、黑名單判斷等共用邏輯

**即時通訊（SignalR Hubs）**
- SignalR 聊天室（`Hubs/ChatHub.cs`）：即時訊息推播、聊天室管理

**Session 與登入驗證**
- Session 管理（`APIControllers/Session`）：Session 權限控管、身分切換

**後台管理（Admin Area）**
- 訊息管理（`Areas/Admin/Controllers/Messages`）：站內私訊、群組訊息管理
- 金流管理（`Areas/Admin/Controllers/FinanceAdmin`）：金流報表、交易查詢、退款管理
- 共用後台控制器（`Controllers/SuperController.cs`）：後台共用邏輯、權限驗證

---
## ⚠️ 注意事項：開發前請確認設定檔與資料庫

為保護敏感資訊，`appsettings.json` 未上傳至 GitHub，請依以下方式準備：

-  請自行建立 `appsettings.json`
-  若需正式設定檔或測試資料庫（`.bak`），請聯絡作者取得
-  本專案使用 SQL Server + Entity Framework Core，可透過 Migration 或備份檔還原資料庫


開發階段搭配 Swagger 進行測試，可於 `/swagger` 查看 API 文件。
---
##  前端對應專案

本專案 API 由前端 Vue 3 專案串接使用，前端包含聊天室 UI、金流流程、會員登入等功能。

 [前往前端倉庫（Vue 3）](https://github.com/aaron5ching/GeeYeangSoreVue)

原始專案為團隊共同開發，本版本為個人備份與展示用途，非正式對外服務平台。
