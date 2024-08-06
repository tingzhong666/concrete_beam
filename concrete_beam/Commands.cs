using Bentley.DgnPlatformNET;
using Bentley.DgnPlatformNET.Elements;
using Bentley.GeometryNET;
using Bentley.MstnPlatformNET;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace concrete_beam
{
    class Commands
    {
        private static DgnModel dgnModel = null;
        private static DgnFile dgnFile = null;
        private static double _1m = 0;
        private static double _1mm = 0;

        private static double _H, _H1, _H2, _H3, _H4, _B4, _H5, _B3, _B1, _B2, _B;

        public static void OutputSuccess(string unparsed)
        {
            MessageBox.Show("Success");
        }
        // （二）巩固练习
        public static void YuanshenRun(string unparsed)
        {
            //MessageBox.Show("YuanshenRun");
            Init();
            // 侧视图 从左下角逆时针进行 画2D元素
            DPoint3d[] side_view_pts = {
                new DPoint3d(0,0,0),
                new DPoint3d(_B1, 0,0),
                new DPoint3d(_B1, _H2,0),
                new DPoint3d(_B3+_B, _H2+_H5,0 ),
                new DPoint3d(_B3+_B, _H2+_H5+_H3, 0),
                new DPoint3d(_B3+_B+_B4, _H2+_H5+_H3+_H4, 0),
                new DPoint3d(_B3+_B+_B4, _H,0),
                new DPoint3d(_B3-_B4, _H, 0),
                new DPoint3d(_B3-_B4, _H2+_H5+_H3+_H4, 0),
                new DPoint3d(_B3, _H2+_H5+_H3, 0),
                new DPoint3d(_B3, _H2+_H5, 0),
                new DPoint3d(0, _H2, 0)
            };

            var side_view_e = new ShapeElement(dgnModel, null, side_view_pts);
            side_view_e.AddToModel();


            // 拉伸
            var result_3d = SurfaceOrSolidElement.CreateProjectionElement(dgnModel, null, side_view_e, new DPoint3d(0, 0, 0), new DVector3d(0, 0, 12000 * _1mm), DTransform3d.Identity, true);
            var dTransform3D = new DTransform3d(new DMatrix3d(1, 0, 0, 0, 0, 1, 0, 1, 0));
            result_3d.ApplyTransform(new TransformInfo(dTransform3D));
            result_3d.AddToModel();


            // 标注 B2 B4 B B4
            dimCreate(side_view_pts[6], side_view_pts[7], new DTransform3d[] { dTransform3D }, (int)(200 * _1mm), 'x'); // B2
            dimCreate(side_view_pts[8], side_view_pts[9], new DTransform3d[] { dTransform3D }, (int)(225 * _1mm), 'x'); // B4
            dimCreate(side_view_pts[4], side_view_pts[9], new DTransform3d[] { dTransform3D }, (int)(300 * _1mm), 'x'); // B
            dimCreate(side_view_pts[4], side_view_pts[5], new DTransform3d[] { dTransform3D }, (int)(300 * _1mm), 'x'); // B4

            // 文本
            textCreate("side view");
        }
        public static void Init()
        {
            dgnModel = dgnModel ?? Session.Instance.GetActiveDgnModel();
            dgnFile = dgnFile ?? Session.Instance.GetActiveDgnFile();
            _1m = _1m == 0 ? Session.Instance.GetActiveDgnModel().GetModelInfo().UorPerMeter : _1m;
            _1mm = _1m * 0.001;

            //长度参数
            _H = 700 * _1mm;
            _H1 = _H2 = 125 * _1mm;
            _H3 = 275 * _1mm;
            _H4 = _B4 = 75 * _1mm;
            _H5 = 100 * _1mm;
            _B3 = 125 * _1mm;
            _B1 = 400 * _1mm;
            _B2 = 300 * _1mm;
            _B = 150 * _1mm;
        }

        // 标注创建
        /// <summary>
        /// 创建标注
        /// </summary>
        /// <param name="p1"></param>
        /// <param name="p2"></param>
        /// <param name="dTransform3Ds">标注的变换</param>
        /// <param name="height">高度</param>
        /// <param name="mode">测量轴 x|y|z</param>
        public static void dimCreate(DPoint3d p1, DPoint3d p2, DTransform3d[] dTransform3Ds, int height, char mode)
        {
            // - 标注样式 缩放 文本宽高 最小 角度 角精度 对齐
            var dimStyle = new DimensionStyle("DimStyle", dgnFile);
            dimStyle.SetBooleanProp(true, DimStyleProp.Placement_UseStyleAnnotationScale_BOOLINT);
            dimStyle.SetDoubleProp(1, DimStyleProp.Placement_AnnotationScale_DOUBLE);
            dimStyle.SetBooleanProp(true, DimStyleProp.Text_OverrideHeight_BOOLINT);
            dimStyle.SetDistanceProp(25 * _1mm, DimStyleProp.Text_Height_DISTANCE, dgnModel);
            dimStyle.SetBooleanProp(true, DimStyleProp.Text_OverrideWidth_BOOLINT);
            dimStyle.SetDistanceProp(15 * _1mm, DimStyleProp.Text_Width_DISTANCE, dgnModel);
            dimStyle.SetBooleanProp(true, DimStyleProp.General_UseMinLeader_BOOLINT);
            dimStyle.SetDoubleProp(0.01, DimStyleProp.Terminator_MinLeader_DOUBLE);
            dimStyle.SetBooleanProp(true, DimStyleProp.Value_AngleMeasure_BOOLINT);
            dimStyle.SetAccuracyProp((Byte)AnglePrecision.Use1Place, DimStyleProp.Value_AnglePrecision_INTEGER);
            dimStyle.SetIntegerProp((int)DimStyleProp_General_Alignment.True, DimStyleProp.General_Alignment_INTEGER);
            // - 文本样式
            var textStyle = new DgnTextStyle("textStyle", dgnFile);
            // - 图层
            var levelId = Settings.GetLevelIdFromName("Default");
            // - 创建标注
            var dimStyleCall = new DimensionCreateCallback(dimStyle, textStyle, new Symbology(), levelId, null);
            var dimE = new DimensionElement(dgnModel, dimStyleCall, DimensionType.SizeStroke);
            if (!dimE.IsValid)
            {
                MessageBox.Show("标注无效");
                return;
            }

            dimE.InsertPoint(p1, null, dimStyle, -1);
            dimE.InsertPoint(p2, null, dimStyle, -1);
            dimE.SetHeight(height);
            switch (mode)
            {
                case 'y':
                    dimE.SetRotationMatrix(new DMatrix3d(0, 1, 0, 1, 0, 0, 0, 0, 1));
                    break;
                case 'z':
                    dimE.SetRotationMatrix(new DMatrix3d(0, 0, 1, 0, 1, 0, 1, 0, 0));
                    break;
                case 'x':
                default:
                    break;
            }
            foreach (DTransform3d dTransform3D in dTransform3Ds)
            {
                dimE.ApplyTransform(new TransformInfo(dTransform3D));
            }
            dimE.AddToModel();
        }

        // 文字
        public static void textCreate(string str)
        {
            var textBlock = new TextBlock(
                new TextBlockProperties(dgnModel),
                new ParagraphProperties(dgnModel),
                new RunProperties(DgnTextStyle.GetSettings(dgnFile), dgnModel),
                dgnModel);
            textBlock.AppendText(str);

            var textE = (TextElement)TextElement.CreateElement(null, textBlock);
            // 缩小 旋转
            textE.ApplyTransform(new TransformInfo(new DTransform3d(DMatrix3d.Multiply(new DMatrix3d(0.05, 0, 0, 0, 0.05, 0, 0, 0, 0.05), new DMatrix3d(1, 0, 0, 0, -1, 0, 0, 0, 1)))));
            // 平移
            textE.ApplyTransform(new TransformInfo(DTransform3d.FromTranslation(new DPoint3d(100 * _1mm, 20 * _1mm, 0))));
            textE.AddToModel();
        }
        public static void test(string unparsed)
        {
            Init();
            // y距离
            var p1 = new DPoint3d(0, 0, 0);
            var p2 = new DPoint3d(0, 0, 150 * _1mm);
            dimCreate(p1, p2, new DTransform3d[] { new DTransform3d(DMatrix3d.Identity) }, 200, 'z');
        }
        // （三）巩固练习
        public static void work3(string unparsed)
        {
            Init();

            // 侧视图 从左下角逆时针进行 画2D元素
            DPoint3d[] side_view_pts = {
                new DPoint3d(0,0,0),
                new DPoint3d(_B1, 0,0),
                new DPoint3d(_B1, _H2,0),
                new DPoint3d(_B3+_B, _H2+_H5,0 ),
                new DPoint3d(_B3+_B, _H2+_H5+_H3, 0),
                new DPoint3d(_B3+_B+_B4, _H2+_H5+_H3+_H4, 0),
                new DPoint3d(_B3+_B+_B4, _H,0),
                new DPoint3d(_B3-_B4, _H, 0),
                new DPoint3d(_B3-_B4, _H2+_H5+_H3+_H4, 0),
                new DPoint3d(_B3, _H2+_H5+_H3, 0),
                new DPoint3d(_B3, _H2+_H5, 0),
                new DPoint3d(0, _H2, 0)
            };

            var side_view_e = new ShapeElement(dgnModel, null, side_view_pts);
            //side_view_e.AddToModel();

            // 拉伸
            var result_3d = SurfaceOrSolidElement.CreateProjectionElement(dgnModel, null, side_view_e, new DPoint3d(0, 0, 0), new DVector3d(0, 0, 12000 * _1mm), DTransform3d.Identity, true);
            var dTransform3D = new DTransform3d(new DMatrix3d(1, 0, 0, 0, 0, 1, 0, 1, 0));
            result_3d.ApplyTransform(new TransformInfo(dTransform3D));

            // 标注 B1 B3 B B3
            dimCreate(side_view_pts[0], side_view_pts[1], new DTransform3d[] { dTransform3D }, (int)(-200 * _1mm), 'x'); // B1
            dimCreate(side_view_pts[0], side_view_pts[10], new DTransform3d[] { dTransform3D }, (int)(-75 * _1mm), 'x'); // B3
            dimCreate(side_view_pts[10], side_view_pts[3], new DTransform3d[] { dTransform3D }, (int)(-300 * _1mm), 'x'); // B
            dimCreate(side_view_pts[3], side_view_pts[1], new DTransform3d[] { dTransform3D }, (int)(-300 * _1mm), 'x'); // B3

            // 标注 H H2 H5 H3 H4 H1
            var rotation_H = DTransform3d.FromRotationAroundLine(new DPoint3d(0, 0, _H / 2), new DVector3d(1, 0, 0), new Angle() { Degrees = 180 });
            var rotation_H2 = DTransform3d.FromRotationAroundLine(new DPoint3d(0, 0, _H2 / 2), new DVector3d(1, 0, 0), new Angle() { Degrees = 180 });
            var rotation_H5 = DTransform3d.FromRotationAroundLine(new DPoint3d(0, 0, (_H2 + _H5) - _H5 / 2), new DVector3d(1, 0, 0), new Angle() { Degrees = 180 });
            var rotation_H3 = DTransform3d.FromRotationAroundLine(new DPoint3d(0, 0, (_H2 + _H5 + _H3) - _H3 / 2), new DVector3d(1, 0, 0), new Angle() { Degrees = 180 });
            var rotation_H4 = DTransform3d.FromRotationAroundLine(new DPoint3d(0, 0, (_H2 + _H5 + _H3 + _H4) - _H4 / 2), new DVector3d(1, 0, 0), new Angle() { Degrees = 180 });
            var rotation_H1 = DTransform3d.FromRotationAroundLine(new DPoint3d(0, 0, _H - _H1 / 2), new DVector3d(1, 0, 0), new Angle() { Degrees = 180 });
            dimCreate(side_view_pts[7], side_view_pts[0], new DTransform3d[] { dTransform3D, rotation_H }, -(int)(200 * _1mm), 'y'); // H 
            dimCreate(side_view_pts[11], side_view_pts[0], new DTransform3d[] { dTransform3D, rotation_H2 }, -(int)(100 * _1mm), 'y'); // H2
            dimCreate(side_view_pts[10], side_view_pts[11], new DTransform3d[] { dTransform3D, rotation_H5 }, -(int)(225 * _1mm), 'y'); // H5
            dimCreate(side_view_pts[9], side_view_pts[10], new DTransform3d[] { dTransform3D, rotation_H3 }, -(int)(225 * _1mm), 'y'); // H3 
            dimCreate(side_view_pts[8], side_view_pts[9], new DTransform3d[] { dTransform3D, rotation_H4 }, -(int)(150 * _1mm), 'y'); // H4
            dimCreate(side_view_pts[7], side_view_pts[8], new DTransform3d[] { dTransform3D, rotation_H1 }, -(int)(150 * _1mm), 'y'); // H1

            // 圆柱
            var cone = new ConeElement(dgnModel, null, 350 * _1mm, 350 * _1mm, new DPoint3d(_B1 / 2, 12000 * _1mm, _H + 200 * _1mm), new DPoint3d(_B1 / 2, 12000 * _1mm, -200 * _1mm), DMatrix3d.Identity, true);

            // 布尔运算
            Convert1.ElementToBody(out var result_3d_body, result_3d, true, false, false);
            Convert1.ElementToBody(out var cone_body, cone, true, false, false);
            var subtrahend = new SolidKernelEntity[] { cone_body };
            Modify.BooleanSubtract(ref result_3d_body, ref subtrahend, 1);
            Convert1.BodyToElement(out var res_e, result_3d_body, null, dgnModel);

            // 材质
            // - 查询材质表并激活
            var materialTable = new MaterialTable(dgnFile) { Name = "qwe" };
            PaletteInfo[] paletteInfos = MaterialManager.GetPalettesInSearchPath("MS_MATERIAL");
            if (paletteInfos.Length == 0)
            {
                MessageBox.Show("MS_MATERIAL路径没得材质表");
                return;
            }
            foreach (var v in paletteInfos)
            {
                if (v.Name == "Concrete&Pavers")
                {
                    materialTable.AddPalette(v);
                    MaterialManager.SetActiveTable(materialTable, dgnModel);
                    MaterialManager.SaveTable(materialTable);
                    break;
                }
            }
            if (materialTable.GetPaletteList().Length == 0)
            {
                MessageBox.Show("MS_MATERIAL下的材质表列表中没有lightwidgets材质表");
                return;
            }

            // - 查询材质
            var materialID = new MaterialId("Concrete new");

            //// -添加此材质
            var elementMaterialProperties = new MaterialPropertiesExtension[] {
                MaterialPropertiesExtension.GetAsMaterialPropertiesExtension((DisplayableElement)res_e),
                MaterialPropertiesExtension.GetAsMaterialPropertiesExtension(cone)
            };
            foreach (var item in elementMaterialProperties)
            {
                item.AddMaterialAttachment(materialID);
                item.StoresAttachmentInfo(materialID);
                item.AddToModel();
            }


            // 文本
            textCreate("side view");
        }

        // 官方work3  start
        // *******************************************************************************************************************************************************************************************
        // *******************************************************************************************************************************************************************************************
        // *******************************************************************************************************************************************************************************************
        #region PracticeWork
        public static void work3_guanfang(string unparsed)
        {
            DgnFile dgnFile = Session.Instance.GetActiveDgnFile();//获得当前激活的文件
            DgnModel dgnModel = Session.Instance.GetActiveDgnModel();//获取当前的模型空间
            double uorPerMeter = Session.Instance.GetActiveDgnModel().GetModelInfo().UorPerMeter;//分辨率单位转换为米

            #region Create beam
            #region Create profile
            double H = 700 * uorPerMeter / 1000;
            double H1 = 125 * uorPerMeter / 1000, H2 = 125 * uorPerMeter / 1000;
            double H3 = 275 * uorPerMeter / 1000;
            double H4 = 75 * uorPerMeter / 1000, B4 = 75 * uorPerMeter / 1000;
            double H5 = 100 * uorPerMeter / 1000;
            double B3 = 125 * uorPerMeter / 1000;
            double B1 = 400 * uorPerMeter / 1000;
            double B2 = 300 * uorPerMeter / 1000;
            double B = 150 * uorPerMeter / 1000;

            DPoint3d p1 = new DPoint3d(-1 * 0.5 * B1, 0, 0);//声明体元素端点
            DPoint3d p2 = new DPoint3d(-1 * 0.5 * B1, 0, H2);
            DPoint3d p3 = new DPoint3d(-0.5 * B, 0, H2 + H5);
            DPoint3d p4 = new DPoint3d(-0.5 * B, 0, H2 + H5 + H3);
            DPoint3d p5 = new DPoint3d(-0.5 * B2, 0, H2 + H5 + H3 + H4);
            DPoint3d p6 = new DPoint3d(-0.5 * B2, 0, H);
            DPoint3d p7 = new DPoint3d(0.5 * B2, 0, H);
            DPoint3d p8 = new DPoint3d(0.5 * B2, 0, H2 + H5 + H3 + H4);
            DPoint3d p9 = new DPoint3d(0.5 * B, 0, H2 + H5 + H3);
            DPoint3d p10 = new DPoint3d(0.5 * B, 0, H2 + H5);
            DPoint3d p11 = new DPoint3d(0.5 * B1, 0, H2);
            DPoint3d p12 = new DPoint3d(0.5 * B1, 0, 0);

            DPoint3d[] pos = { p1, p2, p3, p4, p5, p6, p7, p8, p9, p10, p11, p12 };//将面元素端点添加到面元素端点数组中

            ShapeElement shape = new ShapeElement(dgnModel, null, pos);//声明形元素
            shape.AddToModel();
            #endregion

            DPoint3d origin = DPoint3d.Zero;//声明拉伸基点
            DVector3d extrudeVector = new DVector3d(0, 12 * uorPerMeter, 0);//声明拉伸向量

            SurfaceOrSolidElement beamSolid = SurfaceOrSolidElement.CreateProjectionElement(dgnModel, null, shape, origin, extrudeVector, DTransform3d.Identity, true);//使用投影的方式声明拉伸体元素            
            #endregion

            #region Create dimension
            DPoint3d d1 = new DPoint3d(-0.5 * B1, 0, -50 * uorPerMeter / 1000);//声明标注点
            DPoint3d d2 = new DPoint3d(0.5 * B1, 0, -50 * uorPerMeter / 1000);//声明标注点
            DPoint3d[] dimensionPos1 = { d1, d2 };//声明标注点数组
            DMatrix3d dMatrix1 = new DMatrix3d(-1, 0, 0, 0, 0, 1, 0, -1, 0);//声明变换矩阵
            DimensionElement dimEle1 = CreateDimensionElement(dgnFile, dgnModel, dimensionPos1, string.Empty, dMatrix1);//声明标注元素
            dimEle1.AddToModel();//将标注元素写入模型

            DPoint3d d3 = new DPoint3d(-0.5 * B1, 0, -10 * uorPerMeter / 1000);
            DPoint3d d4 = new DPoint3d(-0.5 * B, 0, -10 * uorPerMeter / 1000);
            DPoint3d d5 = new DPoint3d(0.5 * B, 0, -10 * uorPerMeter / 1000);
            DPoint3d d6 = new DPoint3d(0.5 * B1, 0, -10 * uorPerMeter / 1000);
            DPoint3d[] dimensionPos2 = { d3, d4, d5, d6 };
            DimensionElement dimEle2 = CreateDimensionElement(dgnFile, dgnModel, dimensionPos2, string.Empty, dMatrix1);
            dimEle2.AddToModel();//将标注元素写入模型

            DMatrix3d dMatrix2 = DMatrix3d.FromRows(new DVector3d(0, 1, 0), new DVector3d(-1, 0, 0), new DVector3d(0, 0, 1));
            DMatrix3d dMatrix = DMatrix3d.Multiply(dMatrix1, dMatrix2);

            DPoint3d d7 = new DPoint3d(-0.5 * B1 - 50 * uorPerMeter / 1000, 0, 0);
            DPoint3d d8 = new DPoint3d(-0.5 * B1 - 50 * uorPerMeter / 1000, 0, H);
            DPoint3d[] dimensionPos3 = { d7, d8 };
            DimensionElement dimEle3 = CreateDimensionElement(dgnFile, dgnModel, dimensionPos3, string.Empty, dMatrix);
            dimEle3.AddToModel();//将标注元素写入模型

            DPoint3d d9 = new DPoint3d(-0.5 * B1 - 10 * uorPerMeter / 1000, 0, 0);
            DPoint3d d10 = new DPoint3d(-0.5 * B1 - 10 * uorPerMeter / 1000, 0, H2);
            DPoint3d d11 = new DPoint3d(-0.5 * B1 - 10 * uorPerMeter / 1000, 0, H2 + H5);
            DPoint3d d12 = new DPoint3d(-0.5 * B1 - 10 * uorPerMeter / 1000, 0, H2 + H5 + H3);
            DPoint3d d13 = new DPoint3d(-0.5 * B1 - 10 * uorPerMeter / 1000, 0, H2 + H5 + H3 + H4);
            DPoint3d d14 = new DPoint3d(-0.5 * B1 - 10 * uorPerMeter / 1000, 0, H);
            DPoint3d[] dimensionPos4 = { d9, d10, d11, d12, d13, d14 };
            DimensionElement dimEle4 = CreateDimensionElement(dgnFile, dgnModel, dimensionPos4, string.Empty, dMatrix);
            dimEle4.AddToModel();//将标注元素写入模型
            #endregion

            #region Create column
            EllipseElement ellipse = new EllipseElement(dgnModel, null, DPoint3d.Zero, 350 * uorPerMeter / 1000, 350 * uorPerMeter / 1000, DMatrix3d.Identity);

            DVector3d columnVector = new DVector3d(0, 0, 3 * uorPerMeter);//声明拉伸向量

            SurfaceOrSolidElement columnSolid = SurfaceOrSolidElement.CreateProjectionElement(dgnModel, null, ellipse, DPoint3d.Zero, columnVector, DTransform3d.Identity, true);//使用投影的方式声明拉伸体元素            

            DTransform3d dTransform3D = DTransform3d.FromTranslation(new DPoint3d(0, 12 * uorPerMeter, -1 * uorPerMeter));//声明变换几何，执行元素平移操作
            TransformInfo trans = new TransformInfo(dTransform3D);//声明变换信息
            columnSolid.ApplyTransform(trans);//对拉伸圆柱体施加变换信息

            #endregion

            #region BooleanSubtract
            Convert1.ElementToBody(out SolidKernelEntity entity1, beamSolid, true, false, false);//将实体转成SolidKernelEntity
            Convert1.ElementToBody(out SolidKernelEntity entity2, columnSolid, true, false, false);//将圆台实体元素转成SolidKernelEntity
            SolidKernelEntity[] entities = { entity2 };//声明实核实体集
            Modify.BooleanSubtract(ref entity1, ref entities, entities.Count());//用实核实体集中的实体与实体进行布尔减运算
            Convert1.BodyToElement(out Element resultElem, entity1, null, dgnModel);//将结果转换为元素
            #endregion

            #region Attach material
            MaterialId id = FindMaterial(dgnFile, dgnModel);
            AttachMaterialToElement(id, resultElem);
            AttachMaterialToElement(id, columnSolid);
            #endregion
        }

        private static void AttachMaterialToElement(MaterialId id, Element elem)
        {
            if (id != null)
            {
                MaterialPropertiesExtension propertiesExtension = MaterialPropertiesExtension.GetAsMaterialPropertiesExtension((DisplayableElement)elem);//为拉伸实体元素设置材料属性            
                propertiesExtension.AddMaterialAttachment(id);//添加嵌入的材料信息
                propertiesExtension.StoresAttachmentInfo(id);//保存拉伸实体元素的材料信息           
                propertiesExtension.AddToModel();//将拉伸实体写入模型
            }
        }

        private static MaterialId FindMaterial(DgnFile dgnFile, DgnModel dgnModel)
        {
            MaterialTable matTbl = new MaterialTable(dgnFile);//声明材料表
            matTbl.Name = "MyMaterialTable";//声明材料表名称
            PaletteInfo[] palInfo = MaterialManager.GetPalettesInSearchPath("MS_MATERIAL");//从MS_MATERIAL的环境变量声明路径下读取材料图表
            if (palInfo.Length < 1)//判断是否获取到材料图表
            {
                MessageCenter.Instance.ShowInfoMessage("Can't get palette", null, true);//输出错误信息
                return null;//返回
            }
            for (int i = 0; i < palInfo.Count(); i++)//遍历材料图表
            {
                if (palInfo[i].Name == "Concrete&Pavers")//判断材料图表是否名为Concrete&Pavers
                {
                    matTbl.AddPalette(palInfo[i]);//添加材料图表至材料表
                    break;//跳出循环
                }
                else if (i == palInfo.Count() - 1)//若未找到名为lightwidgets的材料图表
                {
                    MessageCenter.Instance.ShowErrorMessage("Can't find material lib named lightwidgets, please check",
                                                            "Can't find material lib named lightwidgets, please check",
                                                            true);//输出错误信息
                }
            }
            MaterialManager.SetActiveTable(matTbl, dgnModel);//设置当前材料表为激活图表
            MaterialManager.SaveTable(matTbl);//保存材料表

            MaterialId id = new MaterialId("Concrete_1");//查找名为Concrete_1的材料
            return id;
        }
        #endregion

        private static DimensionElement CreateDimensionElement(DgnFile dgnFile, DgnModel dgnModel, DPoint3d[] pos, string text, DMatrix3d dMatrix)
        {
            //获取当前dgn文件中名字为"DimStyle"的标注样式，尺寸标注元素的外貌由上百个属性控制，而标注样式是一组预先设置好的属性
            //获取了预先订制好的标注样式之后，还可以调用DimensionStyle下的各种SetXXX成员函数修改设置的属性
            DimensionStyle dimStyle = new DimensionStyle("DimStyle", dgnFile);//声明标注样式
            dimStyle.SetBooleanProp(true, DimStyleProp.Placement_UseStyleAnnotationScale_BOOLINT);//设置标注样式
            dimStyle.SetDoubleProp(1, DimStyleProp.Placement_AnnotationScale_DOUBLE);
            dimStyle.SetBooleanProp(true, DimStyleProp.Text_OverrideHeight_BOOLINT);
            dimStyle.SetDistanceProp(200, DimStyleProp.Text_Height_DISTANCE, dgnModel);
            dimStyle.SetBooleanProp(true, DimStyleProp.Text_OverrideWidth_BOOLINT);
            dimStyle.SetDistanceProp(200, DimStyleProp.Text_Width_DISTANCE, dgnModel);
            dimStyle.SetBooleanProp(true, DimStyleProp.General_UseMinLeader_BOOLINT);
            dimStyle.SetDoubleProp(0.01, DimStyleProp.Terminator_MinLeader_DOUBLE);
            dimStyle.SetBooleanProp(true, DimStyleProp.Value_AngleMeasure_BOOLINT);
            dimStyle.SetAccuracyProp((byte)AnglePrecision.Use1Place, DimStyleProp.Value_AnglePrecision_INTEGER);
            int alignInt = (int)DimStyleProp_General_Alignment.True;
            StatusInt status = dimStyle.SetIntegerProp(alignInt, DimStyleProp.General_Alignment_INTEGER);
            dimStyle.GetIntegerProp(out int valueOut, DimStyleProp.General_Alignment_INTEGER);
            DgnTextStyle textStyle = new DgnTextStyle("TestStyle", dgnFile);//设置文字样式
            LevelId lvlId = Settings.GetLevelIdFromName("Default");//设置图层

            CreateDimensionCallbacks callbacks = new CreateDimensionCallbacks(dimStyle, textStyle, new Symbology(), lvlId, null);//尺寸标注元素的构造函数会调用DimensionCreateData的各个成员函数去获取声明尺寸标注元素需要的各种参数
            DimensionElement dimEle = new DimensionElement(dgnModel, callbacks, DimensionType.SizeStroke);//声明标注元素
            if (dimEle.IsValid)//判断标注元素是否有效
            {
                for (int i = 0; i < pos.Count(); i++)
                {
                    dimEle.InsertPoint(pos[i], null, dimStyle, -1);//对标注元素设置插入点
                }
                dimEle.SetHeight(500);//设置尺寸标注元素的高度                                
                dimEle.SetRotationMatrix(dMatrix);//设置变换信息
            }
            return dimEle;
        }
    }

    class CreateDimensionCallbacks : DimensionCreateData
    {
        private DimensionStyle m_dimStyle;
        private DgnTextStyle m_textStyle;
        private Symbology m_symbology;
        private LevelId m_levelId;
        private DirectionFormatter m_directionFormatter;
        public CreateDimensionCallbacks(DimensionStyle dimStyle, DgnTextStyle textStyle, Symbology symb, LevelId levelId, DirectionFormatter formatter)
        {
            m_dimStyle = dimStyle;
            m_textStyle = textStyle;
            m_symbology = symb;
            m_levelId = levelId;
            m_directionFormatter = formatter;
        }

        public override DimensionStyle GetDimensionStyle()
        {
            return m_dimStyle;
        }

        public override DgnTextStyle GetTextStyle()
        {
            return m_textStyle;
        }

        public override Symbology GetSymbology()
        {
            return m_symbology;
        }

        public override LevelId GetLevelId()
        {
            return m_levelId;
        }

        public override int GetViewNumber()
        {
            return 0;
        }

        //此函数返回的旋转矩阵与GetViewRotation返回的旋转矩阵共同声明了尺寸标注元素的方向
        public override DMatrix3d GetDimensionRotation()
        {
            return DMatrix3d.Identity;
        }

        public override DMatrix3d GetViewRotation()
        {
            return DMatrix3d.Identity;
        }

        //用于从数字方向值构造字符串。
        public override DirectionFormatter GetDirectionFormatter()
        {
            return m_directionFormatter;
        }
    }

    // 官方work3 end
    // *******************************************************************************************************************************************************************************************
    // *******************************************************************************************************************************************************************************************
    // *******************************************************************************************************************************************************************************************
    // *******************************************************************************************************************************************************************************************
    // *******************************************************************************************************************************************************************************************
    // *******************************************************************************************************************************************************************************************

    class DimensionCreateCallback : DimensionCreateData
    {
        private readonly DimensionStyle dimensionStyle;
        private readonly DgnTextStyle textStyle;
        private readonly Symbology symbology;
        private readonly LevelId levelId;
        private readonly DirectionFormatter directionFormatter;

        public DimensionCreateCallback(DimensionStyle dimensionStyle, DgnTextStyle textStyle, Symbology symbology, LevelId levelId, DirectionFormatter directionFormatter)
        {
            this.dimensionStyle = dimensionStyle;
            this.textStyle = textStyle;
            this.symbology = symbology;
            this.levelId = levelId;
            this.directionFormatter = directionFormatter;
        }
        public override DMatrix3d GetDimensionRotation()
        {
            return DMatrix3d.Identity;
        }

        public override DimensionStyle GetDimensionStyle()
        {
            return this.dimensionStyle;
        }

        public override DirectionFormatter GetDirectionFormatter()
        {
            return this.directionFormatter;
        }

        public override LevelId GetLevelId()
        {
            return this.levelId;
        }

        public override Symbology GetSymbology()
        {
            return this.symbology;
        }

        public override DgnTextStyle GetTextStyle()
        {
            return this.textStyle;
        }

        public override int GetViewNumber()
        {
            return 0;
        }

        public override DMatrix3d GetViewRotation()
        {
            return DMatrix3d.Identity;
        }
    }
}
