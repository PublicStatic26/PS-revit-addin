using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Events;
using Autodesk.Revit.UI.Selection;
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

        public MainForm(UIApplication uiApp)
        {
            InitializeComponent();
            _uiApp = uiApp;
            _doc = uiApp.ActiveUIDocument.Document;
            _eventHandler = new GenericExternalEventHandler();
            _externalEvent = ExternalEvent.Create(_eventHandler);
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
        private void button1_Click(object sender, EventArgs e)  // 이 코드 삭제 금지
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

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {

        }

        private void comboBox3_SelectedIndexChanged(object sender, EventArgs e)
        {
            //Data01 개폐방식
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            //Data02 창호프레임
        }

        private void comboBox2_SelectedIndexChanged(object sender, EventArgs e)
        {
            //Data03 유리종류
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            // 창호 W
        }

        private void textBox2_TextChanged(object sender, EventArgs e)
        {
            // 창호 H
        }

        private void textBox4_TextChanged(object sender, EventArgs e)
        {
            // 문 W
        }

        private void textBox3_TextChanged(object sender, EventArgs e)
        {
            // 문 H
        }

        private void comboBox6_SelectedIndexChanged(object sender, EventArgs e)
        {
            // 문프레임 data
            //** 도어 프레임
            //Aluminum
            //Al + PVC
            //  PVC
            //Combination
            //알루미늄(Aluminum) 경량, 내식성, 가공 용이  상업·공공시설, 커튼월 연계
            //스틸(Steel)  강도 높음, 방화문 프레임 기본 재료 방화문, 비상구, 창고
            //스테인리스(STS) 내식성·내구성 최고, 고가  병원, 클린룸, 고급 로비
            //목재(Wood)   가공 용이, 단열 우수 주거 실내문
            //PVC / uPVC  단열·차음 우수, 저가    주거 창호, 발코니문
            //유리 프레임리스    프레임 최소화, 미관 중시 상업 로비, 오피스

        }

        private void comboBox5_SelectedIndexChanged(object sender, EventArgs e)
        {
            // 현관문 (Entry Door)	외부와 접하는 주 출입구
            // 실내문(Interior Door) 방·복도 사이 내부 칸막이
            // 방화문(Fire Door) 방화구획 경계, 갑종·을종
            // 방화셔터(Fire Shutter) 대형 개구부 자동폐쇄
            // 비상구문(Emergency Exit Door)  피난 법규 적용
            // 방음문(Acoustic Door) 차음 성능 기준 적용
            // 방풍문(Vestibule Door)    외기 차단, 에너지 절감
            // 차고문(Garage Door)   주차장 출입, 오버헤드 방식 多
        }

        private void comboBox4_SelectedIndexChanged(object sender, EventArgs e)
        {
            // 도어 개폐방식
            //** 도어 개폐방식
            //여닫이 문
            //슬라이딩 문
            //턴앤틸트 문
            //여닫이(Swing) 경첩 축으로 회전, 단개 / 양개
            //미서기(Sliding)   좌우 슬라이딩
            //미닫이(Pocket)    벽 속으로 수납
            //접이식(Folding)   병풍식 접개
            //회전식(Revolving) 중심축 회전
            //자동(Automatic)  센서·전동 구동
        }

        private void label8_Click(object sender, EventArgs e)
        {

        }
    }
}





//** 개폐방식 Data01
//고정(Fix)창
//프로젝트 창
//여닫이 창
//슬라이딩 창
//턴앤틸트 창
//리프트슬라이딩 창
//패러럴슬라이딩 창

//** 창호프레임 Data02
//Aluminum
//Al + PVC
//PVC
//Combination
//Curtain Wall
//한식창

//** 유리종류 Data03
//진공유리
//삼중유리
//복층유리
//강화유리
//로이유리
//반사유리
//일반유리



//** 도어 개폐방식
//여닫이 문
//슬라이딩 문
//턴앤틸트 문


//** 도어 프레임
//Aluminum
//Al + PVC
//  PVC
//Combination



//ㅇㅁㄴㅇ
//// 현관도어
// 방화문
// 자동문
