namespace PSRevitAddin.Models
{
    /// <summary>
    /// 창호 / 문 구분
    /// </summary>
    internal enum WindowType
    {
        Window, // 창호
        Door    // 문
    }

    /// <summary>
    /// 유리 종류 (MainForm comboBox2 기준)
    /// </summary>
    internal enum GlassType
    {
        Vacuum,     // 진공유리
        Triple,     // 삼중유리
        Double,     // 복층유리
        Tempered,   // 강화유리
        LowE,       // 로이유리
        Reflective, // 반사유리
        Standard    // 일반유리
    }

    /// <summary>
    /// 창호 프레임 재질 (MainForm comboBox1 기준)
    /// </summary>
    internal enum FrameType
    {
        Aluminum,    // 알루미늄
        AlPvc,       // AL+PVC
        Pvc,         // PVC
        Combination, // 복합
        CurtainWall, // 커튼월
        Traditional  // 한식창
    }

    /// <summary>
    /// 개폐 방식 (MainForm comboBox3 기준)
    /// </summary>
    internal enum OpeningMethod
    {
        Fixed,          // 고정(Fix)창
        ProjectOut,     // 프로젝트창
        CasementSwing,  // 여닫이창
        Sliding,        // 슬라이딩창
        TurnTilt,       // 턴앤틸트창
        LiftSliding,    // 리프트슬라이딩창
        ParallelSliding // 패러럴슬라이딩창
    }
}