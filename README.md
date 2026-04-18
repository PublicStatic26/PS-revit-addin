# PS-revit-addin

> **창호 객체 자동 배치 및 제품 선택 통합 솔루션**  
> CAD 도면에서 창호 정보를 자동으로 읽어 Revit 객체를 생성하고,  
> 실제 제품 DB 기반으로 필터링·선택·견적까지 한 번에 처리하는 Revit Add-in

[![Platform](https://img.shields.io/badge/Platform-Autodesk%20Revit-blue)](https://www.autodesk.com/products/revit/)
[![Language](https://img.shields.io/badge/Language-C%23%20%2F%20.NET%208.0-512BD4)](https://dotnet.microsoft.com/)
[![UI](https://img.shields.io/badge/UI-WinForms-0078D4)](https://learn.microsoft.com/en-us/dotnet/desktop/winforms/)
[![Status](https://img.shields.io/badge/Status-MVP%20In%20Progress-orange)]()

<br><br>

## 배경 — 왜 만들었나요?

건축 실무에서 창호 설계는 단순하지만 반복적이고 오류가 많은 작업입니다.

설계자는 CAD 도면을 보며 창호 유형·사이즈를 수동으로 Revit에 입력하고,  
제품 카탈로그를 직접 뒤져 후보를 선정한 후, 수량과 단가를 수작업으로 계산합니다.  
제품이 바뀌면 그 과정 전체를 처음부터 반복해야 합니다.

이 Add-in은 그 반복을 자동화합니다.

| 기존 방식 | 이 솔루션 |
|---|---|
| CAD 도면 보며 유형·사이즈 수동 입력 | CAD 레이어 기반으로 유형마크·좌표 자동 추출 |
| 벽체·창호를 Revit에 일일이 배치 | 교차점 계산으로 벽체·창호 자동 배치 |
| 카탈로그 직접 검색해 후보 선정 | 제조사·프레임·치수 조건으로 자동 필터링 |
| 수량·단가 수작업 계산 | 일람표 자동 생성 (창호 스펙·단가 일괄 표시) |

<br><br>

## 데모

![PS-revit-addin demo](./demo.gif)

> 4분 22초 전체 워크플로우를 4배속으로 압축한 영상입니다.

<br><br>

## 구현 현황

| 단계 | 기능 | 
|---|---|---|---|
| ① CAD 파싱 | 레이어명 기반 벽체 중심선·창호 블록 추출, 텍스트 파싱 | 
| ② Revit 객체 생성 | 벽체 자동 생성, FamilySymbol 복제·창호 배치 | 
| ③ 제품 DB 조회 | Excel 기반 제품 목록 로드 | 
| ④ 조건 필터 | 제조사·프레임·치수(W×H) AND 필터, 카드 UI 자동 갱신 | 
| ⑤ 제품 선택 | 카드뷰 제품 선택 |
| ⑥ 파라미터 업데이트 | 선택 제품 → Revit 패밀리 파라미터 반영 | 
| ⑦ 일람표 동기화 | 창호 스펙·단가 일람표 자동 생성 | 
| - | GitHub/dev structure | 
| - | UX/UI | 
| - | Grasshopper 연동 | 

<br><br>

## 7단계 워크플로우

```
① CAD 파싱
   레이어명("창생성", "라인")으로 창호·벽체 선 자동 인식
   → 유형마크(BPW), 치수(1500×1200), 씰높이 파싱
   → 텍스트 형식 자동 판별: "BPW_1500x1200" / "21x22x6" / "BPW"

② Revit 객체 생성
   벽체: 중심선 기반 Wall.Create, 200mm 벽 유형 자동 적용
   창호: 벽-창호 교차점 계산 → FamilySymbol 복제 → 삽입
   → 유형마크 단위로 FamilySymbol 복제, 치수·마크 파라미터 자동 입력
   → 씰높이 인스턴스 파라미터 자동 입력

③ 제품 DB 조회
   → Eagon / LX Z:IN / Jinheung 3사 제품 목록 로드

④ 조건 필터
   → 제조사 체크박스, 프레임 유형(알루미늄·PVC 등), 폭×높이 AND 필터
   → Revit에서 선택한 창호 유형의 치수를 자동으로 읽어 필터 조건으로 세팅

⑤ 제품 선택 (진행 중)
   → 카드뷰에서 최종 제품 선택

⑥ 파라미터 업데이트
   → 선택된 제품 정보를 배치된 Revit 창호 객체에 반영
   → 단가·성능 파라미터 자동 입력 (삭제·재생성 없이 파라미터만 교체)

⑦ 일람표 동기화
   → 창호 스펙(방화·단열·개폐방식·유리·프레임) + 단가 일람표 자동 생성
   → 고객 견적 자료로 즉시 활용
```

<br><br>

## 시스템 아키텍처

### 프로젝트 구조

```
PS-revit-addin/
│
├── App.cs                              ← 애드인 시작점 (IExternalApplication)
├── Command.cs                          ← 버튼 클릭 실행 (IExternalCommand)
├── Utility.cs                          ← 공통 유틸 (단위변환, 문자열파싱, 좌표변환)
├── GenericExternalEventHandler.cs      ← Revit UI 스레드 브릿지 (공통 인프라)
│
├── Services/                           ← 핵심 비즈니스 로직
│   ├── CadParser.cs                    ← ① CAD 파싱 (레이어명 기반, 텍스트 형식 자동 판별)
│   ├── FamilyPlacer.cs                 ← ② 벽체·창호 자동 배치 (FamilySymbol 복제)
│   ├── ProductCatalog.cs               ← ③ 제품 목록 조회 (Excel DB)
│   ├── ProductFilter.cs                ← ④ 조건 AND 필터 (제조사·프레임·치수)
│   ├── ProductSelector.cs              ← ⑤ 제품 선택 로직
│   ├── ParameterUpdater.cs             ← ⑥ 파라미터 업데이트 (삭제·재생성 없이 교체)
│   └── ScheduleManager.cs              ← ⑦ 일람표 자동 생성
│
├── Models/                             ← 데이터 구조 (공유 계약, 로직 없음)
│   ├── Enums.cs                        ← FrameType 등 열거형 상수
│   ├── WindowUnit.cs                   ← 창호 정보 (유형마크, 사이즈, 좌표, 선택상태)
│   └── VendorProduct.cs                ← 제품 정보 (회사, 단가, 성능)
│
└── Forms/                              ← WinForm UI
    ├── MainForm.cs
    ├── MainForm.Designer.cs
    └── MainForm.resx
```

### 데이터 흐름

```
CAD 도면 (레이어: "창생성", "라인")
   │ CadParser — 텍스트 형식 자동 판별
   │ "BPW_1500x1200" / "21x22x6" / "BPW"
   ▼
CadParseResult
  ├── WallCenterlines (List<Curve>)
  └── WindowDataList (List<CadWindowData>)
        Mark / Width / Height / SillHeight / Location / CenterLine
   │ FamilyPlacer
   │ 벽-창호 교차점 계산 → FamilySymbol 복제 → 삽입
   ▼
Revit 배치 완료 ─────────────────────────────────────┐
  Wall (200mm) + FamilyInstance (WINDOW-어셈블)      │ Element ID 유지
  유형마크·치수·씰높이 파라미터 자동 입력                    │ 삭제·재생성 금지
   │ ProductCatalog → ProductFilter                │
   │ 제조사·프레임·치수 AND 필터                         │
   ▼                                               │
VendorProduct 후보 목록 (카드뷰 표시)                   │
   │ ProductSelector                               │
   ▼                                               │
선택된 VendorProduct                                 │
   │ ParameterUpdater ◀────────────────────────────┘
   ▼
Revit 파라미터 업데이트 (단가·성능 반영)
   │ ScheduleManager
   ▼
창호 일람표 (스펙·방화·단열·개폐·단가 자동 집계)
```

### Revit UI 스레드 처리 구조

WinForm과 Revit API는 서로 다른 스레드에서 동작합니다.  
`GenericExternalEventHandler`가 이 둘을 안전하게 연결합니다.

```
MainForm (WinForm 스레드)
   │
   │  DozeOff()  ← UI 비활성화
   │  _eventHandler.ActionToExecute = (app) => { Transaction → Service };
   │  _externalEvent.Raise();
   │  Thread.Sleep(100);
   ▼
GenericExternalEventHandler.Execute()
   │  Revit이 준비됐을 때 실행 (IExternalEventHandler)
   ▼
Revit API (Revit UI 스레드)
   Transaction.Start() → Service 호출 → Transaction.Commit()
   │
   ▼
finally: WakeUp()  ← UI 재활성화 (항상 실행)
```

<br><br>

## 핵심 기술 결정 — Why This Approach

### 1. 레이어명 기반 CAD 파싱

AutoCAD 도면의 특정 레이어명(`"창생성"`, `"라인"`)을 기준으로 요소를 구분합니다.  
텍스트는 형식에 따라 자동으로 파싱합니다.

- `"BPW_1500x1200"` → 마크 + 치수 통합
- `"21x22x6"` → 폭×높이×씰높이 (100 미만은 100mm 단위 환산)
- `"BPW"` → 마크만

**이유:** Revit에 Import된 CAD는 블록 구조가 아닌 CurveElement + TextNote로 분해됩니다.  
레이어명과 텍스트 형식이 도면 작성 규칙의 유일한 단서입니다.

### 2. 벽-창호 교차점 계산으로 정밀 배치

창호 중심선과 벽 중심선의 교차점을 계산해 배치 위치를 결정합니다.  
교차점이 없으면 가장 가까운 벽에 투영합니다.  
벽 끝 5% 안쪽으로 삽입점을 클램핑해 경계 오류를 방지합니다.

**이유:** CAD 좌표를 그대로 쓰면 창호가 벽 밖에 배치될 수 있습니다.  
Revit은 창호가 반드시 벽 안에 있어야 배치가 유효합니다.

### 3. FamilySymbol 복제로 유형별 치수 관리

`"WINDOW-어셈블"` 패밀리를 유형마크 단위로 복제(`Duplicate`)해  
각 창호 유형마다 독립된 치수 파라미터를 가진 FamilySymbol을 생성합니다.

**이유:** Revit에서 창호 치수는 유형(Type) 속성입니다.  
같은 패밀리를 쓰면서 치수를 다르게 하려면 유형 복제가 필수입니다.  
이미 같은 이름의 유형이 있으면 재사용해 중복 생성을 방지합니다.

### 4. 객체는 한 번만 생성, 이후는 파라미터 업데이트

제품이 바뀌어도 Revit 창호 객체를 삭제하고 다시 만들지 않습니다.  
파라미터 값만 교체합니다.

**이유:** Revit 일람표는 Element ID로 행을 추적합니다.  
객체를 삭제하면 ID가 사라져 일람표 연속성이 끊깁니다.

### 5. AND 교집합 필터

제조사·프레임·치수 조건은 OR가 아닌 AND로만 처리합니다.  
필터 결과가 0이면 사용자에게 즉시 알립니다.

**이유:** 건축 법규상 창호는 모든 조건을 동시에 만족해야 합니다.  
OR 필터는 법규 미달 제품을 후보에 포함시킬 위험이 있습니다.

### 6. 단일 프로젝트 + 폴더 분리 구조

멀티 프로젝트(`.sln` 내 여러 `.csproj`) 대신,  
단일 프로젝트 내에서 `Services / Models / Forms` 폴더로 역할을 분리했습니다.

**이유:** 팀원 전원이 C# 입문 단계입니다.  
복잡한 프로젝트 참조 구조보다 명확한 폴더 분리가  
학습 비용을 낮추고 협업 충돌을 예방합니다.

<br><br>

## 기술 스택

| 분류 | 기술 |
|---|---|
| 언어 | C# / .NET 8.0 |
| UI 프레임워크 | Windows Forms (WinForms) |
| BIM 플랫폼 | Autodesk Revit API |
| 제품 DB | Excel (추후 확장 예정) |
| 버전 관리 | Git / GitHub |
| 개발 환경 | Visual Studio 2022 (Windows) + VSCode (macOS M2) |
| 추후 연동 예정 | Rhino 8 / Grasshopper |

<br><br>

## 설치 및 실행

### 사전 요구사항

- Autodesk Revit (2024 이상 권장)
- Visual Studio 2022 (Windows)
- .NET 8.0 SDK

### 설치

```bash
git clone https://github.com/PublicStatic26/PS-revit-addin.git
```

1. Visual Studio에서 `.csproj` 파일 열기
2. 빌드 → `PSRevitAddin.dll` 생성 확인
3. `.addin` 파일을 Revit 애드인 폴더에 복사

```
C:\ProgramData\Autodesk\Revit\Addins\2024\
```

4. Revit 재시작 → 리본 탭에서 Add-in 확인

### CAD 도면 작성 규칙

이 Add-in은 도면 레이어명 규칙을 기준으로 파싱합니다.

| 레이어명 | 용도 |
|---|---|
| `라인` 포함 | 벽체 중심선 |
| `창생성` 포함 | 창호 위치선 |
| 텍스트 타입 `최종도면-돋움체-35` | 창호 유형마크·치수 |

텍스트 형식은 세 가지를 지원합니다.

```
BPW_1500x1200   → 마크_폭x높이
21x22x6         → 폭x높이x씰높이 (100 미만 숫자는 100mm 단위)
BPW             → 마크만 (치수는 별도 텍스트에서 파싱)
```

<br><br>

## 개발 환경 구성

이 프로젝트는 **macOS + Windows 혼합 환경**에서 개발됩니다.

| 환경 | 용도 |
|---|---|
| Mac M2 + VSCode + Claude Code | 로직 작성, 코드 리뷰, Git 관리 |
| Windows PC + Visual Studio | WinForm Designer, Revit 빌드·테스트 |

> WinForm 이벤트는 반드시 **Visual Studio Designer에서만** 생성합니다.  
> Mac에서 코드로 직접 이벤트를 연결하면 `Designer.cs`와 불일치가 발생해  
> Windows에서 빌드 오류 또는 Designer 충돌이 생길 수 있습니다.

<br><br>

## 팀

| 이름 | 담당 |
|---|---|
| 손수진 | GitHub/dev structure, UX/UI, ProductFilter.cs, ProductSelector.cs |
| 이태권 | ProductCatalog.cs, ParameterUpdater.cs, ScheduleManager.cs |
| 황혜선 | CadParser.cs, FamilyPlacer.cs |
| 안아영 | Grasshopper |

<br><br>

## 라이선스

This project is developed for academic and portfolio purposes.
