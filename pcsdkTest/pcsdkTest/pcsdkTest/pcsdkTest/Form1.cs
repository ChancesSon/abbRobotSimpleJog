using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;

using ABB.Robotics;
using ABB.Robotics.Controllers;
using ABB.Robotics.Controllers.Discovery;
using ABB.Robotics.Controllers.RapidDomain;
using ABB.Robotics.Controllers.IOSystemDomain;
using ABB.Robotics.Controllers.EventLogDomain;
using ABB.Robotics.Controllers.MotionDomain;
using ABB.Robotics.Controllers.FileSystemDomain;

namespace pcsdkTest
{
    public partial class btnDecrRz : Form
    {
        public btnDecrRz()
        {
            InitializeComponent();
            Control.CheckForIllegalCrossThreadCalls = false;
        }

        private NetworkScanner scanner = null;
        private NetworkWatcher networkwatcher = null;
        private Controller controller = null;
        //private ABB.Robotics.Controllers.RapidDomain.Task[] tasks = null;
        private Mastership master;
        //RapidData a, b;

        private void Form1_Load(object sender, EventArgs e)
        {
            this.scanner = new NetworkScanner();
            this.scanner.Scan();
            ControllerInfoCollection controllers = scanner.Controllers;
            ListViewItem item = null;
            foreach (ControllerInfo controllerInfo in controllers)
            {
                item = new ListViewItem(controllerInfo.IPAddress.ToString());
                item.SubItems.Add(controllerInfo.Id);
                item.SubItems.Add(controllerInfo.Availability.ToString());
                item.SubItems.Add(controllerInfo.IsVirtual.ToString());
                item.SubItems.Add(controllerInfo.SystemName);
                item.SubItems.Add(controllerInfo.Version.ToString());
                item.SubItems.Add(controllerInfo.ControllerName);
                this.listView1.Items.Add(item);
                item.Tag = controllerInfo;
            }
            //Controller aController = new Controller();
            //item = new ListViewItem(aController.IPAddress.ToString());
            //item.SubItems.Add(aController.SystemId.ToString());
            //item.SubItems.Add(aController.Connected.ToString());
            //item.SubItems.Add(aController.IsVirtual.ToString());
            //item.SubItems.Add(aController.SystemName);
            //item.SubItems.Add(aController.RobotWareVersion.ToString());
            ////item.SubItems.Add(controllerInfo.ControllerName);
            //this.listView1.Items.Add(item);
            //item.Tag = aController;

            this.networkwatcher = new NetworkWatcher(scanner.Controllers);
            this.networkwatcher.Found += new EventHandler<NetworkWatcherEventArgs>(HandleFoundEvent);
            this.networkwatcher.Lost += new EventHandler<NetworkWatcherEventArgs>(HandleLostEvent);
            this.networkwatcher.EnableRaisingEvents = true;

        }


        void HandleFoundEvent(object sender, NetworkWatcherEventArgs e)
        {
            this.Invoke(new EventHandler<NetworkWatcherEventArgs>(AddControllerToListView), new Object[] { this, e });
        }

        void HandleLostEvent(object sender, NetworkWatcherEventArgs e)
        {
            this.Invoke(new EventHandler<NetworkWatcherEventArgs>(SubtractControllerFromListView), new Object[] { this, e });
        }


        private void AddControllerToListView(object sender, NetworkWatcherEventArgs e)
        {
            ControllerInfo controllerInfo = e.Controller;
            ListViewItem item = new ListViewItem(controllerInfo.IPAddress.ToString());
            item.SubItems.Add(controllerInfo.Id);
            item.SubItems.Add(controllerInfo.Availability.ToString());
            item.SubItems.Add(controllerInfo.IsVirtual.ToString());
            item.SubItems.Add(controllerInfo.SystemName);
            item.SubItems.Add(controllerInfo.Version.ToString());
            item.SubItems.Add(controllerInfo.ControllerName);
            this.listView1.Items.Add(item);
            item.Tag = controllerInfo;
        }


        private void SubtractControllerFromListView(object sender, NetworkWatcherEventArgs e)
        {
            this.scanner = new NetworkScanner();
            this.scanner.Scan();
            ControllerInfoCollection controllers = scanner.Controllers;
            ListViewItem item = null;
            this.listView1.Items.Clear();
            foreach (ControllerInfo controllerInfo in controllers)
            {
                item = new ListViewItem(controllerInfo.IPAddress.ToString());
                item.SubItems.Add(controllerInfo.Id);
                item.SubItems.Add(controllerInfo.Availability.ToString());
                item.SubItems.Add(controllerInfo.IsVirtual.ToString());
                item.SubItems.Add(controllerInfo.SystemName);
                item.SubItems.Add(controllerInfo.Version.ToString());
                item.SubItems.Add(controllerInfo.ControllerName);
                this.listView1.Items.Add(item);
                item.Tag = controllerInfo;
            }
        }

        private void listView1_DoubleClick(object sender, EventArgs e)
        {
            ListViewItem item = this.listView1.SelectedItems[0];
            if (item.Tag != null)
            {
                ControllerInfo controllerInfo = (ControllerInfo)item.Tag;
                if (controllerInfo.Availability == ABB.Robotics.Controllers.Availability.Available)
                {
                    if (this.controller != null)
                    {
                        this.controller.Logoff();
                        this.controller.Dispose();
                        this.controller = null;
                    }
                    this.controller = ControllerFactory.CreateFrom(controllerInfo);
                    //this.controller = new Controller(controllerInfo);
                    MessageBox.Show("Selected controller available.");

                    System.Threading.Thread th1 = new System.Threading.Thread(showRobotPosition);
                    th1.IsBackground = true;
                    th1.Start();



                    this.controller.Logon(UserInfo.DefaultUser);
                    tbOPmode.Text = this.controller.OperatingMode.ToString();
                    tbState.Text = this.controller.State.ToString();





                    this.controller.OperatingModeChanged += new EventHandler<OperatingModeChangeEventArgs>(OperationModeChanged);
                    this.controller.StateChanged += new EventHandler<StateChangedEventArgs>(StateChanged);


                    
                }
                else
                {
                    MessageBox.Show("Selected controller not available.");
                }
            }
            //showEventLog();
        }


        private void showRobotPosition()
        {
            while (true)
            {
                try
                {
                    JointTarget currentJointPoint = this.controller.MotionSystem.ActiveMechanicalUnit.GetPosition();
                    tbJ1.Text = Math.Round(currentJointPoint.RobAx.Rax_1, 2).ToString();
                    tbJ2.Text = Math.Round(currentJointPoint.RobAx.Rax_2, 2).ToString();
                    tbJ3.Text = Math.Round(currentJointPoint.RobAx.Rax_3, 2).ToString();
                    tbJ4.Text = Math.Round(currentJointPoint.RobAx.Rax_4, 2).ToString();
                    tbJ5.Text = Math.Round(currentJointPoint.RobAx.Rax_5, 2).ToString();
                    tbJ6.Text = Math.Round(currentJointPoint.RobAx.Rax_6, 2).ToString();


                    RobTarget currentCartesianPoint = this.controller.MotionSystem.ActiveMechanicalUnit.GetPosition(ABB.Robotics.Controllers.MotionDomain.CoordinateSystemType.World);
                    tbX.Text = Math.Round(currentCartesianPoint.Trans.X, 2).ToString();
                    tbY.Text = Math.Round(currentCartesianPoint.Trans.Y, 2).ToString();
                    tbZ.Text = Math.Round(currentCartesianPoint.Trans.Z, 2).ToString();

                    double q0, q1, q2, q3;
                    //double Rz,Ry,Rx;
                    q0 = currentCartesianPoint.Rot.Q1;
                    q1 = currentCartesianPoint.Rot.Q2;
                    q2 = currentCartesianPoint.Rot.Q3;
                    q3 = currentCartesianPoint.Rot.Q4;
                    //Rz = Math.Atan(2 * (q1 * q2 - q0 * q3) / (q0 * q0 + q1 * q1 - q2 * q2 - q3 * q3));
                    //Ry = Math.Asin(-2*(q0*q2+q1*q3));
                    //Rx = Math.Atan(2 * (q2 * q3 - q0 * q1) / (q0 * q0 - q1 * q1 - q2 * q2 + q3 * q3));
                    //tbQ1.Text = Rx.ToString();
                    //tbQ2.Text = Ry.ToString();
                    //tbQ3.Text = Rz.ToString();
                    tbQ1.Text = Math.Round(q0, 2).ToString();
                    tbQ2.Text = Math.Round(q1, 2).ToString();
                    tbQ3.Text = Math.Round(q2, 2).ToString();
                    tbQ4.Text = Math.Round(q3, 2).ToString();
                }
                catch
                { }
            }
        }


        private void OperationModeChanged(object sender, OperatingModeChangeEventArgs e)
        {
            tbOPmode.Text = this.controller.OperatingMode.ToString();
        }



        private void StateChanged(object sender, StateChangedEventArgs e)
        {
            tbState.Text = this.controller.State.ToString();
        }

        


        private void btPowerOff_Click(object sender, EventArgs e)
        {
            if (this.controller.State == ControllerState.MotorsOn)
            {
                this.controller.State = ControllerState.MotorsOff;
            }
        }

        private void btPowerOn_Click(object sender, EventArgs e)
        {
            if (this.controller.State == ControllerState.MotorsOff)
            {
                this.controller.State = ControllerState.MotorsOn;
            }
        }

        private void btEmergency_Click(object sender, EventArgs e)
        {
            try
            {
                if (this.controller.Rapid.ExecutionStatus == ExecutionStatus.Running)
                {
                    this.controller.State = ControllerState.EmergencyStop;
                }
            }
            catch (System.Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }

        private void btEmergencyReset_Click(object sender, EventArgs e)
        {
            try
            {
                if (this.controller.State == ControllerState.EmergencyStop)
                {
                    this.controller.State = ControllerState.EmergencyStopReset;
                }
            }
            catch (System.Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }

        private void Jogging(string joint, float OffsJointValue)
        {
            JointTarget jCurPos = this.controller.MotionSystem.ActiveMechanicalUnit.GetPosition();
            switch (joint)
            {
                case "J1":
                    jCurPos.RobAx.Rax_1 += OffsJointValue;
                    break;
                case "J2":
                    jCurPos.RobAx.Rax_2 += OffsJointValue;
                    break;
                case "J3":
                    jCurPos.RobAx.Rax_3 += OffsJointValue;
                    break;
                case "J4":
                    jCurPos.RobAx.Rax_4 += OffsJointValue;
                    break;
                case "J5":
                    jCurPos.RobAx.Rax_5 += OffsJointValue;
                    break;
                case "J6":
                    jCurPos.RobAx.Rax_6 += OffsJointValue;
                    break;
            }
            try
            {
                using (this.master = Mastership.Request(controller.Rapid))
                {
                    RapidData rd = controller.Rapid.GetRapidData("T_ROB1", "Module1", "jTarget");
                    controller.Rapid.GetTask("T_ROB1").SetProgramPointer("Module1", "moveJoint");
                    rd.Value = jCurPos;
                    controller.Rapid.Start();
                }
                if (this.controller.Rapid.ExecutionStatus != ExecutionStatus.Running)
                {
                    master.Release();
                }
            }
            catch (System.InvalidOperationException ex)
            {
                MessageBox.Show("Mastership is held by another client." + ex.Message);
                master.Release();
                master = Mastership.Request(controller);
            }
            catch (System.Exception ex)
            {
                MessageBox.Show("Unexpected error occurred: " + ex.Message);
            }
        }


        private void LinearJogging(string Direction, float OffsJointValue)
        {
            RobTarget CurrentCartesianPoint = this.controller.MotionSystem.ActiveMechanicalUnit.GetPosition(ABB.Robotics.Controllers.MotionDomain.CoordinateSystemType.World);
            double q0, q1, q2, q3;
            //double Rz, Ry, Rx;
            q0 = CurrentCartesianPoint.Rot.Q1;
            q1 = CurrentCartesianPoint.Rot.Q2;
            q2 = CurrentCartesianPoint.Rot.Q3;
            q3 = CurrentCartesianPoint.Rot.Q4;
            //Rz = Math.Atan(2 * (q1 * q2 - q0 * q3) / (q0 * q0 + q1 * q1 - q2 * q2 - q3 * q3));
            //Ry = Math.Asin(-2 * (q0 * q2 + q1 * q3));
            //Rx = Math.Atan(2 * (q2 * q3 - q0 * q1) / (q0 * q0 - q1 * q1 - q2 * q2 + q3 * q3));

            switch (Direction)
            {
                case "X":
                    CurrentCartesianPoint.Trans.X += OffsJointValue;
                    break;
                case "Y":
                    CurrentCartesianPoint.Trans.Y += OffsJointValue;
                    break;
                case "Z":
                    CurrentCartesianPoint.Trans.Z += OffsJointValue;
                    break;
                case "Q1":
                    CurrentCartesianPoint.Rot.Q1 += OffsJointValue;
                    break;
                case "Q2":
                    CurrentCartesianPoint.Rot.Q2 += OffsJointValue;
                    break;
                case "Q3":
                    CurrentCartesianPoint.Rot.Q3 += OffsJointValue;
                    break;
                case "Q4":
                    CurrentCartesianPoint.Rot.Q4 += OffsJointValue;
                    break;
                    //case "Rx":
                    //    Rx += OffsJointValue;
                    //    break;
                    //case "Ry":
                    //    Ry += OffsJointValue;
                    //    break;
                    //case "Rz":
                    //    Rz += OffsJointValue;
                    //    break;
            }

            //CurrentCartesianPoint.Rot.Q1 = Math.Cos(Rz / 2) * Math.Cos(Ry / 2) * Math.Cos(Rx / 2) - Math.Sin(Rz / 2) * Math.Sin(Ry / 2) * Math.Sin(Rx / 2);
            //CurrentCartesianPoint.Rot.Q2 = Math.Cos(Rz / 2) * Math.Cos(Ry / 2) * Math.Sin(Rx / 2) + Math.Sin(Rz / 2) * Math.Sin(Ry / 2) * Math.Cos(Rx / 2);
            //CurrentCartesianPoint.Rot.Q3 = Math.Cos(Rz / 2) * Math.Sin(Ry / 2) * Math.Cos(Rx / 2) - Math.Sin(Rz / 2) * Math.Cos(Ry / 2) * Math.Sin(Rx / 2);
            //CurrentCartesianPoint.Rot.Q4 = Math.Sin(Rz / 2) * Math.Cos(Ry / 2) * Math.Cos(Rx / 2) + Math.Cos(Rz / 2) * Math.Sin(Ry / 2) * Math.Sin(Rx / 2);

            try
            {
                using (this.master = Mastership.Request(controller.Rapid))
                {
                    RapidData rd = controller.Rapid.GetRapidData("T_ROB1", "Module1", "pTarget");
                    controller.Rapid.GetTask("T_ROB1").SetProgramPointer("Module1", "MoveLinear");
                    rd.Value = CurrentCartesianPoint;
                    controller.Rapid.Start();
                }
                if (this.controller.Rapid.ExecutionStatus != ExecutionStatus.Running)
                {
                    master.Release();
                }
            }
            catch (System.InvalidOperationException ex)
            {
                MessageBox.Show("Mastership is held by another client." + ex.Message);
                master.Release();
                master = Mastership.Request(controller);
            }
            catch (System.Exception ex)
            {
                MessageBox.Show("Unexpected error occurred: " + ex.Message);
            }
        }

        private void btnIncrJ1_Click(object sender, EventArgs e)
        {
            Jogging("J1", +5);
        }

        private void btnDecrJ1_Click(object sender, EventArgs e)
        {
            Jogging("J1", -5);
        }

        private void btnIncrJ2_Click(object sender, EventArgs e)
        {
            Jogging("J2", +5);
        }

        private void btnDecrJ2_Click(object sender, EventArgs e)
        {
            Jogging("J2", -5);
        }

        private void btnIncrJ3_Click(object sender, EventArgs e)
        {
            Jogging("J3", +5);
        }

        private void btnDecrJ3_Click(object sender, EventArgs e)
        {
            Jogging("J3", -5);
        }

        private void btnIncrJ4_Click(object sender, EventArgs e)
        {
            Jogging("J4", +5);
        }

        private void btnDecrJ4_Click(object sender, EventArgs e)
        {
            Jogging("J4", -5);
        }

        private void btnIncrJ5_Click(object sender, EventArgs e)
        {
            Jogging("J5", +5);
        }

        private void btnDecrJ5_Click(object sender, EventArgs e)
        {
            Jogging("J5", -5);
        }

        private void btnIncrJ6_Click(object sender, EventArgs e)
        {
            Jogging("J6", +5);
        }

        private void btnDecrJ6_Click(object sender, EventArgs e)
        {
            Jogging("J6", -5);
        }

        private void btnIncrX_Click(object sender, EventArgs e)
        {
            LinearJogging("X", +5);
        }

        private void btnDecrX_Click(object sender, EventArgs e)
        {
            LinearJogging("X", -5);
        }

        private void btnIncrY_Click(object sender, EventArgs e)
        {
            LinearJogging("Y", +5);
        }

        private void btnDecrY_Click(object sender, EventArgs e)
        {
            LinearJogging("Y", -5);
        }

        private void btnIncrZ_Click(object sender, EventArgs e)
        {
            LinearJogging("Z", +5);
        }

        private void btnDecrZ_Click(object sender, EventArgs e)
        {
            LinearJogging("Z", -5);
        }

        private void btnIncrQ1_Click(object sender, EventArgs e)
        {
            LinearJogging("Q1", +0.1f);
        }

        private void btnDecrQ1_Click(object sender, EventArgs e)
        {
            LinearJogging("Q1", -0.1f);
        }

        private void btnIncrQ2_Click(object sender, EventArgs e)
        {
            LinearJogging("Q2", +0.1f);
        }

        private void btnDecrQ2_Click(object sender, EventArgs e)
        {
            LinearJogging("Q2", -0.1f);
        }

        private void btnIncrQ3_Click(object sender, EventArgs e)
        {
            LinearJogging("Q3", +0.1f);
        }

        private void btnDecrQ3_Click(object sender, EventArgs e)
        {
            LinearJogging("Q3", -0.1f);
        }

        private void btnIncrQ4_Click(object sender, EventArgs e)
        {
            LinearJogging("Q4", +0.1f);
        }


        private void btnDecrQ4_Click(object sender, EventArgs e)
        {
            LinearJogging("Q4", -0.1f);
        }

        private void btnHome_Click(object sender, EventArgs e)
        {
            // JointTarget jCurPos = this.controller.MotionSystem.ActiveMechanicalUnit.GetPosition();
            JointTarget jCurPos=new JointTarget();
            jCurPos.FillFromString2("[[0,0,0,0,90,0],[9E9,9E9,9E9,9E9,9E9,9E9]]");
 
            try
            {
                using (this.master = Mastership.Request(controller.Rapid))
                {
                    RapidData rd = controller.Rapid.GetRapidData("T_ROB1", "Module1", "jTarget");
                    controller.Rapid.GetTask("T_ROB1").SetProgramPointer("Module1", "moveJoint");
                    rd.Value = jCurPos;
                    controller.Rapid.Start();
                }
                if (this.controller.Rapid.ExecutionStatus != ExecutionStatus.Running)
                {
                    master.Release();
                }
            }
            catch (System.InvalidOperationException ex)
            {
                MessageBox.Show("Mastership is held by another client." + ex.Message);
                master.Release();
                master = Mastership.Request(controller);
            }
            catch (System.Exception ex)
            {
                MessageBox.Show("Unexpected error occurred: " + ex.Message);
            }
        }
    }
}
