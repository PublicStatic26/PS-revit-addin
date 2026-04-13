using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using PSRevitAddin.Models;
using PSRevitAddin.Services;
using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;
using TaskDialog = Autodesk.Revit.UI.TaskDialog;

namespace PSRevitAddin.Forms
{
    public partial class MainForm : System.Windows.Forms.Form
    {
        private UIApplication _uiApp;
        private Document _doc;
        private GenericExternalEventHandler _eventHandler;
        private ExternalEvent? _externalEvent;
        private ProductFilter _productFilter;
        private readonly ProductCatalog _catalog;

        public MainForm(UIApplication uiApp)
        {
            InitializeComponent();
            _uiApp = uiApp;
            _doc = uiApp.ActiveUIDocument.Document;
            _eventHandler = new GenericExternalEventHandler();
            _externalEvent = ExternalEvent.Create(_eventHandler);
            _productFilter = new ProductFilter();
            _catalog = new ProductCatalog();
            InitializeComboBoxes();
        }

        /// <summary>
        /// 콤보박스에 항목을 채운다.
        /// 항목 순서는 Enums.cs의 enum 값 순서와 반드시 일치해야 한다.
        /// SelectedIndex를 enum으로 직접 캐스팅하기 때문이다.
        /// </summary>
        private void InitializeComboBoxes()
        {
            // 창호 프레임 (FrameType 순서와 일치)
            comboBox1.Items.Add("알루미늄 (Aluminum)");
            comboBox1.Items.Add("AL + PVC");
            comboBox1.Items.Add("PVC");
            comboBox1.Items.Add("복합 (Combination)");
            comboBox1.Items.Add("커튼월 (Curtain Wall)");
            comboBox1.Items.Add("한식창");

            // 유리 종류 (GlassType 순서와 일치)
            comboBox2.Items.Add("진공유리");
            comboBox2.Items.Add("삼중유리");
            comboBox2.Items.Add("복층유리");
            comboBox2.Items.Add("강화유리");
            comboBox2.Items.Add("로이유리");
            comboBox2.Items.Add("반사유리");
            comboBox2.Items.Add("일반유리");

            // 개폐 방식 (OpeningMethod 순서와 일치)
            comboBox3.Items.Add("고정(Fix)창");
            comboBox3.Items.Add("프로젝트창");
            comboBox3.Items.Add("여닫이창");
            comboBox3.Items.Add("슬라이딩창");
            comboBox3.Items.Add("턴앤틸트창");
            comboBox3.Items.Add("리프트슬라이딩창");
            comboBox3.Items.Add("패러럴슬라이딩창");

            // 도어 개폐 방식 (OpeningMethod 중 문에 해당하는 항목)
            comboBox4.Items.Add("여닫이 문");
            comboBox4.Items.Add("슬라이딩 문");
            comboBox4.Items.Add("턴앤틸트 문");

            // 도어 기능 및 용도
            comboBox5.Items.Add("현관문");
            comboBox5.Items.Add("실내문");
            comboBox5.Items.Add("방화문");
            comboBox5.Items.Add("방화셔터");
            comboBox5.Items.Add("비상구문");
            comboBox5.Items.Add("방음문");
            comboBox5.Items.Add("방풍문");
            comboBox5.Items.Add("차고문");

            // 문 프레임 (FrameType 중 문에 해당하는 항목)
            comboBox6.Items.Add("알루미늄 (Aluminum)");
            comboBox6.Items.Add("AL + PVC");
            comboBox6.Items.Add("PVC");
            comboBox6.Items.Add("복합 (Combination)");

            // 제품 목록 ListView 컬럼 설정
            //listView4.View = System.Windows.Forms.View.Details;
            //listView4.FullRowSelect = true;
            //listView4.Columns.Add("제조사", 75);
            //listView4.Columns.Add("제품명", 90);
            //listView4.Columns.Add("모델번호", 70);
            //listView4.Columns.Add("단가(원)", 75);

            RefreshProductCards();
        }

        /// <summary>
        /// 현재 필터 조건으로 제품 목록을 갱신한다.
        /// 모든 필터 이벤트 핸들러 끝에서 호출한다.
        /// </summary>
        private void RefreshProductCards()
        {
            var filtered = _productFilter.Apply(_catalog.GetAll());
            //listView4.Items.Clear();

            if (filtered.Count == 0)
            {
                //listView4.Items.Add(new ListViewItem("조건에 맞는 제품이 없습니다."));
                return;
            }

            foreach (var product in filtered)
            {
                var item = new ListViewItem(product.VendorName);
                item.SubItems.Add(product.ProductName);
                item.SubItems.Add(product.ModelNumber);
                item.SubItems.Add($"{product.UnitPrice:N0}");
                //listView4.Items.Add(item);
            }
        }

        #region 단일쓰레드 안정성 확보
        public void DozeOff()
        {
            EnableCommands(false);
        }
        public void WakeUp()
        {
            EnableCommands(true);
        }
        private void EnableCommands(bool status)
        {
            foreach (System.Windows.Forms.Control ctrl in this.Controls)
            {
                ctrl.Enabled = status;
            }
        }

        #endregion


        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            // ExternalEvent 정리
            if (_externalEvent != null)
            {
                _externalEvent.Dispose();
                _externalEvent = null;
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            try
            {
                DozeOff();
                _eventHandler.ActionToExecute = (app) =>
                {
                    Document doc = app.ActiveUIDocument.Document;
                    using (Transaction trans = new Transaction(doc, "ex"))
                    {
                        trans.Start();
                        TaskDialog.Show("Test", "이건머지?");
                        trans.Commit();
                    }
                };

                // ExternalEvent 실행
                _externalEvent?.Raise();
                // 잠시 대기
                System.Threading.Thread.Sleep(100);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error applying colors:\n{ex.Message}",
                "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                WakeUp();
            }
        }

        // ─── Window 탭 이벤트 (try/catch 패턴 — Revit 미접촉) ──────────

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            // 방화 조건
            try
            {
                _productFilter.FilterFireRated = checkBox1.Checked;
                RefreshProductCards();
            }
            catch (Exception ex)
            {
                MessageBox.Show("오류:\n" + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void checkBox2_CheckedChanged(object sender, EventArgs e)
        {
            // 단열 조건
            try
            {
                _productFilter.FilterInsulated = checkBox2.Checked;
                RefreshProductCards();
            }
            catch (Exception ex)
            {
                MessageBox.Show("오류:\n" + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void checkBox3_CheckedChanged(object sender, EventArgs e)
        {
            // 삼중유리 조건
            // 체크 시 SelectedGlass를 Triple로 고정하고 comboBox2를 비활성화한다
            // 해제 시 comboBox2 선택값으로 복원한다
            try
            {
                if (checkBox3.Checked)
                {
                    _productFilter.SelectedGlass = GlassType.Triple;
                    comboBox2.Enabled = false;
                }
                else
                {
                    comboBox2.Enabled = true;
                    _productFilter.SelectedGlass = comboBox2.SelectedIndex >= 0
                        ? (GlassType?)comboBox2.SelectedIndex
                        : null;
                }
                RefreshProductCards();
            }
            catch (Exception ex)
            {
                MessageBox.Show("오류:\n" + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            // 창호 프레임 조건
            try
            {
                _productFilter.SelectedFrame = comboBox1.SelectedIndex >= 0
                    ? (FrameType?)comboBox1.SelectedIndex
                    : null;
                RefreshProductCards();
            }
            catch (Exception ex)
            {
                MessageBox.Show("오류:\n" + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void comboBox2_SelectedIndexChanged(object sender, EventArgs e)
        {
            // 유리 종류 조건
            // 삼중유리 체크박스가 체크된 상태면 콤보박스 선택은 무시한다
            try
            {
                if (checkBox3.Checked)
                {
                    return;
                }

                _productFilter.SelectedGlass = comboBox2.SelectedIndex >= 0
                    ? (GlassType?)comboBox2.SelectedIndex
                    : null;
                RefreshProductCards();
            }
            catch (Exception ex)
            {
                MessageBox.Show("오류:\n" + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void comboBox3_SelectedIndexChanged(object sender, EventArgs e)
        {
            // 창호 개폐방식 조건
            try
            {
                _productFilter.SelectedOpening = comboBox3.SelectedIndex >= 0
                    ? (OpeningMethod?)comboBox3.SelectedIndex
                    : null;
                RefreshProductCards();
            }
            catch (Exception ex)
            {
                MessageBox.Show("오류:\n" + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            // 창호 너비(W) 입력
            try
            {
                bool isValid = double.TryParse(textBox1.Text, out double width);
                _productFilter.TargetWidthMm = isValid && width > 0 ? width : 0;
                RefreshProductCards();
            }
            catch (Exception ex)
            {
                MessageBox.Show("오류:\n" + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void textBox2_TextChanged(object sender, EventArgs e)
        {
            // 창호 높이(H) 입력
            try
            {
                bool isValid = double.TryParse(textBox2.Text, out double height);
                _productFilter.TargetHeightMm = isValid && height > 0 ? height : 0;
                RefreshProductCards();
            }
            catch (Exception ex)
            {
                MessageBox.Show("오류:\n" + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // ─── Door 탭 이벤트 (try/catch 패턴 — Revit 미접촉) ────────────

        private void comboBox4_SelectedIndexChanged(object sender, EventArgs e)
        {
            // 도어 개폐방식 조건
            // comboBox4 항목은 OpeningMethod 중 일부(여닫이, 슬라이딩, 턴앤틸트)만 포함한다
            OpeningMethod[] doorOpeningMethods =
            [
                OpeningMethod.CasementSwing,
                OpeningMethod.Sliding,
                OpeningMethod.TurnTilt
            ];

            try
            {
                _productFilter.SelectedOpening = comboBox4.SelectedIndex >= 0
                    ? doorOpeningMethods[comboBox4.SelectedIndex]
                    : null;
                RefreshProductCards();
            }
            catch (Exception ex)
            {
                MessageBox.Show("오류:\n" + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void comboBox5_SelectedIndexChanged(object sender, EventArgs e)
        {
            // 도어 기능/용도 선택
            // VendorProduct 모델에 도어 용도 필드가 추가되면 필터 조건에 연결한다
            try
            {
                // TODO: VendorProduct에 DoorPurpose 필드 추가 후 구현
            }
            catch (Exception ex)
            {
                MessageBox.Show("오류:\n" + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void comboBox6_SelectedIndexChanged(object sender, EventArgs e)
        {
            // 문 프레임 조건
            // comboBox6 항목은 FrameType 중 앞 4개(Aluminum, AlPvc, Pvc, Combination)와 순서가 일치한다
            try
            {
                _productFilter.SelectedFrame = comboBox6.SelectedIndex >= 0
                    ? (FrameType?)comboBox6.SelectedIndex
                    : null;
                RefreshProductCards();
            }
            catch (Exception ex)
            {
                MessageBox.Show("오류:\n" + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void textBox4_TextChanged(object sender, EventArgs e)
        {
            // 문 너비(W) 입력
            try
            {
                bool isValid = double.TryParse(textBox4.Text, out double width);
                _productFilter.TargetWidthMm = isValid && width > 0 ? width : 0;
                RefreshProductCards();
            }
            catch (Exception ex)
            {
                MessageBox.Show("오류:\n" + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void textBox3_TextChanged(object sender, EventArgs e)
        {
            // 문 높이(H) 입력
            try
            {
                bool isValid = double.TryParse(textBox3.Text, out double height);
                _productFilter.TargetHeightMm = isValid && height > 0 ? height : 0;
                RefreshProductCards();
            }
            catch (Exception ex)
            {
                MessageBox.Show("오류:\n" + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void label8_Click(object sender, EventArgs e)
        {

        }

        private void label3_Click(object sender, EventArgs e)
        {

        }

        private void flowLayoutPanel1_Paint(object sender, PaintEventArgs e)
        {

        }

        private void checkBox6_CheckedChanged(object sender, EventArgs e)
        {
            // 제조사 A
        }

        private void comboBox7_SelectedIndexChanged(object sender, EventArgs e)
        {
            // 창호 유형
        }

        private void textBox5_TextChanged(object sender, EventArgs e)
        {
            // b
        }

        private void checkBox7_CheckedChanged(object sender, EventArgs e)
        {
            // 제조사 B
        }

        private void checkBox9_CheckedChanged(object sender, EventArgs e)
        {
            // 제조사 C
        }

        private void comboBox8_SelectedIndexChanged(object sender, EventArgs e)
        {

        }
    }
}
