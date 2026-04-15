
using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using PSRevitAddin.Models;
using PSRevitAddin.Services;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using TaskDialog = Autodesk.Revit.UI.TaskDialog;
using Panel = System.Windows.Forms.Panel;
using Color = System.Drawing.Color;
using Point = System.Drawing.Point;

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
        private List<VendorProduct> _allProducts = new List<VendorProduct>();
        private VendorProduct? _selectedProduct = null;

        private static readonly string[] VendorNames = { "Eagon", "LX Z:IN", "Jinheung" };

        public MainForm(UIApplication uiApp)
        {
            InitializeComponent();
            _uiApp = uiApp;
            _doc = uiApp.ActiveUIDocument.Document;
            _eventHandler = new GenericExternalEventHandler();
            _externalEvent = ExternalEvent.Create(_eventHandler);
            _productFilter = new ProductFilter();
            _catalog = new ProductCatalog(@"Z:\5조\창호DB.xlsx");
            _allProducts = _catalog.GetAllProducts();
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

            RefreshProductCards();
        }

        /// <summary>
        /// 현재 필터 조건으로 제품 목록을 갱신한다.
        /// 모든 필터 이벤트 핸들러 끝에서 호출한다.
        /// </summary>
        private void RefreshProductCards()
        {
            List<VendorProduct> filtered = _productFilter.Apply(_allProducts);

            // 3사 고정 그룹 초기화
            Dictionary<string, List<VendorProduct>> grouped = new Dictionary<string, List<VendorProduct>>();
            foreach (string name in VendorNames)
                grouped[name] = new List<VendorProduct>();
            foreach (VendorProduct p in filtered)
                if (grouped.ContainsKey(p.VendorName))
                    grouped[p.VendorName].Add(p);

            flowLayoutPanel1.SuspendLayout();
            flowLayoutPanel1.Controls.Clear();
            flowLayoutPanel1.FlowDirection = FlowDirection.LeftToRight;
            flowLayoutPanel1.WrapContents = false;
            flowLayoutPanel1.AutoScroll = false;
            flowLayoutPanel1.Padding = new Padding(0);

            int totalH = flowLayoutPanel1.ClientSize.Height > 100 ? flowLayoutPanel1.ClientSize.Height : 548;
            int totalW = flowLayoutPanel1.ClientSize.Width > 100 ? flowLayoutPanel1.ClientSize.Width : 674;
            int secW = (totalW - VendorNames.Length * 4) / VendorNames.Length;
            int secH = totalH - 4;

            foreach (string vendorName in VendorNames)
            {
                Panel section = CreateVendorSection(vendorName, grouped[vendorName], secW, secH);
                flowLayoutPanel1.Controls.Add(section);
            }

            flowLayoutPanel1.ResumeLayout();
        }

        /// <summary>
        /// 제조사 이름과 제품 목록을 받아 헤더 + 제품 카드로 구성된 섹션 패널을 반환한다.
        /// </summary>
        private Panel CreateVendorSection(string vendorName, List<VendorProduct> products, int width, int height)
        {
            const int headerH = 28;

            const int cardH = 190;

            const int cardGap = 4;

            Panel section = new Panel();
            section.Width = width;
            section.Height = height;
            section.Margin = new Padding(2, 2, 2, 2);
            section.BorderStyle = BorderStyle.FixedSingle;

            // 제조사 헤더
            Label header = new Label();
            header.Text = vendorName;
            header.Location = new Point(0, 0);
            header.Size = new Size(width, headerH);
            header.Font = new Font(this.Font, FontStyle.Bold);
            header.BackColor = Color.SteelBlue;
            header.ForeColor = Color.White;
            header.TextAlign = ContentAlignment.MiddleLeft;
            header.Padding = new Padding(8, 0, 0, 0);
            section.Controls.Add(header);

            // 제품 카드 스크롤 영역
            Panel scrollArea = new Panel();
            scrollArea.Location = new Point(0, headerH);
            scrollArea.Size = new Size(width, height - headerH);
            scrollArea.AutoScroll = true;

            int yOffset = 4;
            foreach (VendorProduct product in products)
            {
                Panel card = CreateProductCard(product, width - SystemInformation.VerticalScrollBarWidth - 8);
                card.Location = new Point(4, yOffset);
                scrollArea.Controls.Add(card);
                yOffset += cardH + cardGap;
            }

            section.Controls.Add(scrollArea);
            return section;
        }

        /// <summary>
        /// 개별 제품 정보를 카드 형태의 패널로 반환한다.
        /// </summary>
        private Panel CreateProductCard(VendorProduct product, int width)
        {

            int cardHeight = 190;
            const int rowH = 18;
            const int rowStride = 22; // rowH(18) + gap(4)
            const int startY = 8;


            Panel card = new Panel();
            card.Width = width;
            card.Height = cardHeight;
            card.BackColor = Color.WhiteSmoke;
            card.BorderStyle = BorderStyle.FixedSingle;
            card.Cursor = Cursors.Hand;
            card.Click += (s, e) => SelectProduct(product, card);
            card.Tag = product;


            // 1행: 제품명 (굵게)
            Label nameLabel = new Label();
            nameLabel.Text = product.ProductName;
            nameLabel.Location = new Point(8, startY + rowStride * 0);
            nameLabel.Size = new Size(width - 16, rowH);
            nameLabel.Font = new Font(this.Font, FontStyle.Bold);
            card.Controls.Add(nameLabel);

            // 2행: 단열 / 방화 / 전동개폐 여부
            string insulated = product.IsInsulated ? "단열 ✔" : "단열 ✘";
            string fireRated = product.IsFireRated ? "방화 ✔" : "방화 ✘";
            string autoOpen = product.IsAutoOpening ? "전동개폐 ✔" : "전동개폐 ✘";

            Label badgeLabel = new Label();
            badgeLabel.Text = insulated + "  " + fireRated + "  " + autoOpen;
            badgeLabel.Location = new Point(8, startY + rowStride * 1);
            badgeLabel.Size = new Size(width - 16, rowH);
            badgeLabel.ForeColor = Color.DimGray;
            card.Controls.Add(badgeLabel);

            // 3행: 창호 프레임
            Label frameLabel = new Label();
            frameLabel.Text = "프레임: " + FrameTypeToKorean(product.FrameType);
            frameLabel.Location = new Point(8, startY + rowStride * 2);
            frameLabel.Size = new Size(width - 16, rowH);
            frameLabel.ForeColor = Color.DimGray;
            card.Controls.Add(frameLabel);

            // 4행: 유리 종류
            Label glassLabel = new Label();
            glassLabel.Text = "유리: " + GlassTypeToKorean(product.GlassType);
            glassLabel.Location = new Point(8, startY + rowStride * 3);
            glassLabel.Size = new Size(width - 16, rowH);
            glassLabel.ForeColor = Color.DimGray;
            card.Controls.Add(glassLabel);

            // 5행: 개폐방식
            Label openingLabel = new Label();
            openingLabel.Text = "개폐: " + OpeningMethodToKorean(product.OpeningMethod);
            openingLabel.Location = new Point(8, startY + rowStride * 4);
            openingLabel.Size = new Size(width - 16, rowH);
            openingLabel.ForeColor = Color.DimGray;
            card.Controls.Add(openingLabel);

            // 6행: 최대치수
            Label maxSizeLabel = new Label();
            maxSizeLabel.Text = "최대: W " + product.MaxWidthMm.ToString("0") + " × H " + product.MaxHeightMm.ToString("0") + " mm";
            maxSizeLabel.Location = new Point(8, startY + rowStride * 5);
            maxSizeLabel.Size = new Size(width - 16, rowH);
            maxSizeLabel.ForeColor = Color.DimGray;
            card.Controls.Add(maxSizeLabel);

            // 7행: 최소치수
            Label minSizeLabel = new Label();
            minSizeLabel.Text = "최소: W " + product.MinWidthMm.ToString("0") + " × H " + product.MinHeightMm.ToString("0") + " mm";
            minSizeLabel.Location = new Point(8, startY + rowStride * 6);
            minSizeLabel.Size = new Size(width - 16, rowH);
            minSizeLabel.ForeColor = Color.DimGray;
            card.Controls.Add(minSizeLabel);

            // 8행: 단가 (오른쪽 정렬, 굵게)
            Label priceLabel = new Label();
            priceLabel.Text = "₩ " + product.UnitPrice.ToString("N0");
            priceLabel.Location = new Point(8, startY + rowStride * 7);
            priceLabel.Size = new Size(width - 16, rowH);

            priceLabel.TextAlign = ContentAlignment.MiddleRight;
            priceLabel.Font = new Font(this.Font, FontStyle.Bold);
            priceLabel.ForeColor = Color.DarkBlue;
            card.Controls.Add(priceLabel);

            return card;
        }

        private Panel? _selectedCard = null;

        private void SelectProduct(VendorProduct product, Panel card)
        {
            if (_selectedCard != null)
                _selectedCard.BackColor = Color.WhiteSmoke;

            _selectedProduct = product;
            _selectedCard = card;
            card.BackColor = Color.LightSteelBlue;
        }

        private string GlassTypeToKorean(GlassType glassType)
        {
            switch (glassType)
            {
                case GlassType.Vacuum: return "진공유리";
                case GlassType.Triple: return "삼중유리";
                case GlassType.Double: return "복층유리";
                case GlassType.Tempered: return "강화유리";
                case GlassType.LowE: return "로이유리";
                case GlassType.Reflective: return "반사유리";
                case GlassType.Standard: return "일반유리";
                default: return glassType.ToString();
            }
        }

        private string FrameTypeToKorean(FrameType frameType)
        {
            switch (frameType)
            {
                case FrameType.Aluminum: return "알루미늄";
                case FrameType.AlPvc: return "AL+PVC";
                case FrameType.Pvc: return "PVC";
                case FrameType.Combination: return "복합";
                case FrameType.CurtainWall: return "커튼월";
                case FrameType.Traditional: return "한식창";
                default: return frameType.ToString();
            }
        }

        private string OpeningMethodToKorean(OpeningMethod openingMethod)
        {
            switch (openingMethod)
            {
                case OpeningMethod.Fixed: return "고정창";
                case OpeningMethod.ProjectOut: return "프로젝트창";
                case OpeningMethod.CasementSwing: return "여닫이창";
                case OpeningMethod.Sliding: return "슬라이딩창";
                case OpeningMethod.TurnTilt: return "턴앤틸트창";
                case OpeningMethod.LiftSliding: return "리프트슬라이딩창";
                case OpeningMethod.ParallelSliding: return "패러럴슬라이딩창";
                default: return openingMethod.ToString();
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
            // 자동 개폐 조건
            try
            {
                _productFilter.FilterAutoOpening = checkBox3.Checked;
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
            try
            {
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



        private void flowLayoutPanel1_Paint(object sender, PaintEventArgs e)
        {
            // 3사 비교표
        }

        private void checkBox6_CheckedChanged(object sender, EventArgs e)
        {
            // 제조사 A 필터
            try
            {
                ToggleVendorFilter("Eagon", checkBox6.Checked);
                RefreshProductCards();
            }
            catch (Exception ex)
            {
                MessageBox.Show("오류:\n" + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
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
            // 제조사 B 필터
            try
            {
                ToggleVendorFilter("LX Z:IN", checkBox7.Checked);
                RefreshProductCards();
            }
            catch (Exception ex)
            {
                MessageBox.Show("오류:\n" + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void checkBox9_CheckedChanged(object sender, EventArgs e)
        {
            // 제조사 C 필터
            try
            {
                ToggleVendorFilter("Jinheung", checkBox9.Checked);
                RefreshProductCards();
            }
            catch (Exception ex)
            {
                MessageBox.Show("오류:\n" + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// 제조사 이름을 SelectedVendors 목록에 추가하거나 제거한다.
        /// checked=true이면 추가, false이면 제거.
        /// 중복 추가는 방지한다.
        /// </summary>
        private void ToggleVendorFilter(string vendorName, bool isChecked)
        {
            if (isChecked)
            {
                if (!_productFilter.SelectedVendors.Contains(vendorName))
                {
                    _productFilter.SelectedVendors.Add(vendorName);
                }
            }
            else
            {
                _productFilter.SelectedVendors.Remove(vendorName);
            }
        }

        private void comboBox8_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void button4_Click(object sender, EventArgs e)
        {
            try
            {
                if (comboBox7.SelectedIndex < 0)
                {
                    MessageBox.Show("창호유형을 선택해주세요.", "알림", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                if (_selectedProduct == null)
                {
                    MessageBox.Show("제품을 선택해주세요.", "알림", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                string selectedTypeName = comboBox7.SelectedItem?.ToString() ?? "";
                if (string.IsNullOrEmpty(selectedTypeName)) return;

                _selectedProduct.SymbolCode = selectedTypeName;
                var productToApply = _selectedProduct;

                DozeOff();
                _eventHandler.ActionToExecute = (app) =>
                {
                    Document doc = app.ActiveUIDocument.Document;
                    var updater = new ParameterUpdater(doc);
                    updater.UpdateFamilyType([productToApply]);
                };

                _externalEvent?.Raise();
                System.Threading.Thread.Sleep(100);
                MessageBox.Show("파라미터 적용 완료!", "완료", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"오류:\n{ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                WakeUp();
            }
        }





        private void MainForm_Load(object sender, EventArgs e)
        {

        }

        private void button2_Click_1(object sender, EventArgs e)
        {
            try
            {
                DozeOff();
                _eventHandler.ActionToExecute = (app) =>
                {
                    UIDocument uiDoc = app.ActiveUIDocument;
                    Document doc = uiDoc.Document;

                    // 1. 프로젝트 내 기본 창호 패밀리 로드 (필수)
                    var defaultWindowSymbol = new FilteredElementCollector(doc)
                        .OfCategory(BuiltInCategory.OST_Windows)
                        .OfClass(typeof(FamilySymbol))
                        .Cast<FamilySymbol>()
                        .FirstOrDefault(s => s.Name == "WINDOW-어셈블") ?? // "WINDOW-어셈블" 우선 검색
                        new FilteredElementCollector(doc)
                        .OfCategory(BuiltInCategory.OST_Windows)
                        .OfClass(typeof(FamilySymbol))
                        .Cast<FamilySymbol>()
                        .FirstOrDefault(); // 없으면 첫 번째 창호

                    if (defaultWindowSymbol == null)
                    {
                        System.Windows.Forms.MessageBox.Show("프로젝트에 로드된 창호 패밀리가 없습니다.", "오류");
                        return;
                    }

                    // 2. 도면 파싱 실행 (PickObject 없이 바로 스캔)
                    CadParser parser = new CadParser(doc); // 주의: 클래스 멤버 _doc 대신 지역 변수 doc 사용
                    CadParseResult parsedData = parser.ParseAllCadData();

                    // 파싱된 데이터가 없는 경우 방어 로직 (사용자가 분해를 안 했을 때)
                    if (parsedData.WallCenterlines.Count == 0 && parsedData.WindowDataList.Count == 0)
                    {
                        System.Windows.Forms.MessageBox.Show(
                            "추출된 도면 데이터가 없습니다.\n\n캐드 도면을 클릭하고 '부분 분해(Partial Explode)'를 먼저 실행해 주세요.",
                            "알림"
                        );
                        return;
                    }

                    // 3. 3D 모델 자동 배치 실행
                    // (FamilyPlacer 내부에 Transaction이 있으므로 밖에서 using Transaction 안 씀!)
                    FamilyPlacer placer = new FamilyPlacer(doc);
                    placer.ExecutePlacement(parsedData, defaultWindowSymbol);

                    System.Windows.Forms.MessageBox.Show(
                        $"작업 완료!\n\n- 인식된 벽체 선: {parsedData.WallCenterlines.Count}개\n- 배치된 창호: {parsedData.WindowDataList.Count}개",
                        "성공"
                    );
                };

                // ExternalEvent 실행
                _externalEvent?.Raise();
                // 잠시 대기
                System.Threading.Thread.Sleep(100);
            }
            catch (Exception ex)
            {
                System.Windows.Forms.MessageBox.Show($"오류 발생:\n{ex.Message}", "Error", System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Error);
            }
            finally
            {
                WakeUp();
            }
        }

private void checkBox8_CheckedChanged(object sender, EventArgs e)
        {

        }
    }
}