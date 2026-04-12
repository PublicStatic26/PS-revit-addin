# PSRevitAddin - Claude Code 컨텍스트

## 프로젝트 개요

CAD 기반 창호 설계 자동화 + 제품 선택 + 견적 시스템
Revit 안에서 CAD를 읽어 창호 객체를 자동 생성하고, 실제 제품 DB 기반으로 견적과 고객 설득 자료를 만드는 Revit 애드인

## 기술 스택

- C# / .NET 8.0
- Revit API
- WinForm
- Visual Studio / VSCode
- Rhino8 / Grasshopper (추후)

## 팀 구성

- 팀장 (Soojin): 코드 담당, C# Revit 애드인 입문자
- 팀원 1~3명: 개발 경험 전무

## 네임스페이스

```
PSRevitAddin
```

폴더 구조와 네임스페이스를 일치시킨다.

```
Services/CadParser.cs   →   namespace PSRevitAddin.Services
Models/WindowUnit.cs    →   namespace PSRevitAddin.Models
Forms/MainForm.cs       →   namespace PSRevitAddin.Forms
```

## 폴더 구조

```
PS-revit-addin/
├── App.cs                              ← 애드인 시작점 (IExternalApplication)
├── Command.cs                          ← 버튼 클릭 실행 (IExternalCommand)
├── Utility.cs                          ← 공통 유틸 (단위변환, 문자열파싱, 좌표변환)
│
├── Services/                           ← 핵심 기능 로직
│   ├── CadParser.cs                    ← ① CAD에서 유형마크 + 좌표 추출
│   ├── FamilyPlacer.cs                 ← ② Revit 기본 창호 객체 생성
│   ├── ProductCatalog.cs               ← ③ Excel에서 제품 목록 조회
│   ├── ProductFilter.cs                ← ④ 방화/단열/유리 조건 AND 필터
│   ├── ProductSelector.cs              ← ⑤ 콤보박스에서 제품 선택
│   ├── ParameterUpdater.cs             ← ⑥ 실제 패밀리 교체 + 파라미터 입력
│   ├── ScheduleManager.cs              ← ⑦ 일람표 수량×단가 자동 집계
│   └── GenericExternalEventHandler.cs  ← Revit UI 스레드 중간 다리
│
├── Models/                             ← 데이터 구조 (로직 없음)
│   ├── WindowUnit.cs                   ← 창호 정보 (유형마크, 사이즈, 좌표, 선택상태)
│   └── VendorProduct.cs                ← 제품 정보 (회사, 단가, 성능)
│
└── Forms/                              ← WinForm UI
    ├── MainForm.cs
    ├── MainForm.Designer.cs
    └── MainForm.resx
```

## 전체 사용자 흐름

```
① Revit에 임포트된 CAD에서 창호 블록 클릭
   → 유형마크(WD1) + 사이즈(1200x1500) + 좌표 자동 추출

② 추출된 정보 기준으로
   → 같은 위치에 Revit 기본 창호 객체 자동 생성
   → 일람표에 행 확보 + 유형마크 입력
   → 상태: 제품 미선택

③ 생성된 창호 사이즈 기준으로
   → A사 5개, B사 4개 제품 후보 화면에 표시

④ 사용자가 체크박스 클릭
   → 방화 ✔ 단열 ✔ 선택
   → A사 2개, B사 1개로 후보 축소

⑤ 사용자가 버튼을 통해 최종 N사 제품 선택

⑥ Revit에 배치된 모든 WD1 기본 객체
   → N사 실제 제품 패밀리로 교체
   → 실제 창호 모양 + 스펙 모델링 반영
   → 단가 + 성능 파라미터 자동 입력

⑦ 일람표 자동 업데이트
   → 수량 × 단가 자동 계산
   → 회사별 견적 비교표 완성
   → 고객 설득 자료로 바로 활용
```

## 브랜치 전략

| 브랜치 | 담당 서비스 | 기능 |
|---|---|---|
| `main` | - | 항상 안정적인 상태 유지 |
| `feature/cad-parse` | CadParser.cs | ① CAD 파싱 |
| `feature/revit-place-family` | FamilyPlacer.cs | ② 기본 객체 생성 |
| `feature/db-query` | ProductCatalog.cs | ③ 제품 목록 조회 |
| `feature/filter-condition` | ProductFilter.cs | ④ 조건 필터 |
| `feature/product-select` | ProductSelector.cs | ⑤ 제품 선택 |
| `feature/param-update` | ParameterUpdater.cs | ⑥ 패밀리 교체 + 파라미터 입력 |
| `feature/schedule-sync` | ScheduleManager.cs | ⑦ 일람표 자동화 |

## 브랜치 작업 규칙

1. `feature/브랜치명` 생성
2. 작업
3. PR 올리기
4. 팀원 코드 리뷰
5. `main` 머지

> main에 직접 push 금지. 반드시 PR을 통해 머지한다.

## 핵심 설계 원칙

### 1. 단일 책임 (Single Responsibility)

클래스 하나는 역할 하나만 담당한다.

- `CadParser.cs`는 파싱만
- `FamilyPlacer.cs`는 배치만
- UI 로직을 Service에 섞지 않는다

### 2. 명명 규칙 (Naming Convention)

| 대상 | 규칙 | 예시 |
|---|---|---|
| 클래스명 | PascalCase | `CadParser` |
| 메서드명 | PascalCase | `ParseBlock()` |
| 변수명 | camelCase | `typeCode` |
| 상수 | UPPER_SNAKE_CASE | `MAX_WIDTH` |
| private 필드 | _camelCase | `_revitDoc` |

### 3. null 처리

Revit API는 null을 자주 반환한다. null 체크 없이 사용하면 Revit 자체가 크래시된다.

```csharp
// 나쁜 예
var element = doc.GetElement(id);
element.Name = "test"; // null이면 크래시

// 좋은 예
var element = doc.GetElement(id);
if (element == null) return;
element.Name = "test";
```

### 4. Revit Transaction 원칙

Revit 객체를 수정할 때는 반드시 Transaction 안에서만 가능하다.

```csharp
using (Transaction tx = new Transaction(doc, "작업명"))
{
    tx.Start();
    // Revit 객체 수정
    tx.Commit();
}
```

### 5. using 블록 사용

Transaction, 파일 읽기 등 리소스는 반드시 `using`으로 감싸서 자동 해제한다.

### 6. 메서드 크기

메서드 하나는 한 가지 일만 한다. 길어지면 쪼갠다.

### 7. 주석 원칙

- 코드가 **무엇을 하는지(What)** 는 코드로 표현
- 코드가 **왜 이렇게 했는지(Why)** 는 주석으로 표현
- 당연한 것은 주석 달지 않는다

### 8. 객체 생성 원칙 (이 프로젝트 전용)

- Revit 창호 객체는 ②에서 딱 한 번만 생성한다
- ⑥에서 삭제 후 재생성 금지 → 파라미터 값만 교체
- 이유: 일람표 연속성 보장, Element ID 유실 방지

### 9. 데이터 흐름 원칙 (이 프로젝트 전용)

- CAD 추출 → Revit 반영 순서를 반드시 지킨다
- 확정되지 않은 데이터를 Revit에 반영하지 않는다
- 위치/형태는 ②에서 확정, 제품 정보는 ⑥에서 확정

### 10. 필터 원칙 (이 프로젝트 전용)

- 조건은 AND 교집합으로만 처리
- 필터 결과가 0개면 사용자에게 반드시 알림 표시

### 11. 일람표 원칙 (이 프로젝트 전용)

- 일람표는 항상 Revit 객체 기준으로 자동 계산
- 수동 입력 금지, 수량 × 단가는 자동 계산