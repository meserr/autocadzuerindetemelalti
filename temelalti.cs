using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.AutoCAD.Runtime;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Colors;
using Autodesk.AutoCAD.GraphicsInterface;
using Topology.IO.Dwg;
using Topology.Geometries;
namespace autocad_test
{
    public class CommandMethods
    {
        public void Initialize()
        {
            SystemObjects.DynamicLinker.LoadModule(
                "AcMPolygonObj" + Application.Version.Major + ".dbx", false, false);
        }
        List<entity> entitylist = new List<entity>();
        entity en = new entity();
        double cembercap;
        int solagidilen;
        List<int> globalSolagidilen;
        bool solagidilmedurumu = false;
        int globalyatayakssayisi = 0;
        int globaldikeyakssayisi = 0;
        List<Point3d> kesismenoktalari;
        List<Point3d> Xaksnoktalaribaslangic;
        List<Point3d> Xaksnoktalaribitis;
        List<Point3d> Yaksnoktalaribaslangic;
        List<Point3d> Yaksnoktalaribitis;
        List<Point3d> cemberListesi;
        List<Point3d> yukaricemberler;
        Point3d ilkCember;
        List<double> ynoktalari = new List<double>();
        List<double> xnoktalari = new List<double>();

        bool durumx = true;
        bool durumy = true;
        int yerel_y_c_sayisi;
        int d_c_sayisi;
        double ybaslanıcnoktasi;
        List<int> global_y_c_sayisi;
        double soldangirinti;
        double yukaridanGirinti;
        private void BitisCemberleriX_ust(List<Point3d> list, string text, string konum)
        {
            int i = 1;
            Document acDoc = Application.DocumentManager.MdiActiveDocument;
            Database acCurDb = acDoc.Database;
            using (Transaction acTrans = acCurDb.TransactionManager.StartTransaction())
            {
                BlockTable acBlkTbl;
                acBlkTbl = acTrans.GetObject(acCurDb.BlockTableId, OpenMode.ForRead) as BlockTable;
                BlockTableRecord acBlkTblRec;
                acBlkTblRec = acTrans.GetObject(acBlkTbl[BlockTableRecord.ModelSpace], OpenMode.ForWrite) as BlockTableRecord;

                foreach (var item in list)
                {
                    switch (konum)
                    {
                        case "xust":
                            using (Circle acCirc = new Circle())
                            {
                                acCirc.Center = new Point3d(item.X, item.Y + 10, item.Z);
                                acCirc.Diameter = 80;
                                acCirc.Color = Color.FromColorIndex(ColorMethod.ByBlock, 1);
                                acBlkTblRec.AppendEntity(acCirc);
                                acTrans.AddNewlyCreatedDBObject(acCirc, true);
                            }
                            using (DBText acText = new DBText())
                            {
                                acText.Position = new Point3d(item.X - 30, item.Y - 10, item.Z);
                                acText.Height = 30;
                                acText.TextString = text + "" + i.ToString();
                                acBlkTblRec.AppendEntity(acText);
                                acTrans.AddNewlyCreatedDBObject(acText, true);
                            }
                            break;
                        case "xalt":
                            using (Circle acCirc = new Circle())
                            {
                                acCirc.Center = new Point3d(item.X, item.Y - 10, item.Z);
                                acCirc.Diameter = 80;
                                acCirc.Color = Color.FromColorIndex(ColorMethod.ByBlock, 1);
                                acBlkTblRec.AppendEntity(acCirc);
                                acTrans.AddNewlyCreatedDBObject(acCirc, true);
                            }
                            using (DBText acText = new DBText())
                            {
                                acText.Position = new Point3d(item.X - 20, item.Y - 20, item.Z);
                                acText.Height = 30;
                                acText.TextString = text + "" + i.ToString();
                                acBlkTblRec.AppendEntity(acText);
                                acTrans.AddNewlyCreatedDBObject(acText, true);
                            }
                            break;
                        case "ysol":
                            using (Circle acCirc = new Circle())
                            {
                                acCirc.Center = new Point3d(item.X - 10, item.Y, item.Z);
                                acCirc.Diameter = 80;
                                acCirc.Color = Color.FromColorIndex(ColorMethod.ByBlock, 1);
                                acBlkTblRec.AppendEntity(acCirc);
                                acTrans.AddNewlyCreatedDBObject(acCirc, true);
                            }
                            using (DBText acText = new DBText())
                            {
                                acText.Position = new Point3d(item.X - 40, item.Y - 25, item.Z);
                                acText.Height = 30;
                                acText.TextString = text + "" + i.ToString();
                                acBlkTblRec.AppendEntity(acText);
                                acTrans.AddNewlyCreatedDBObject(acText, true);
                            }
                            break;
                        case "ysag":
                            using (Circle acCirc = new Circle())
                            {
                                acCirc.Center = new Point3d(item.X + 10, item.Y, item.Z);
                                acCirc.Diameter = 80;
                                acCirc.Color = Color.FromColorIndex(ColorMethod.ByBlock, 1);
                                acBlkTblRec.AppendEntity(acCirc);
                                acTrans.AddNewlyCreatedDBObject(acCirc, true);
                            }
                            using (DBText acText = new DBText())
                            {
                                acText.Position = new Point3d(item.X - 20, item.Y - 15, item.Z);
                                acText.Height = 30;
                                acText.TextString = text + "" + i.ToString();
                                acBlkTblRec.AppendEntity(acText);
                                acTrans.AddNewlyCreatedDBObject(acText, true);
                            }
                            break;

                        default:
                            break;
                    }

                    i++;
                }
                acTrans.Commit();
            }
        }
        private bool IsPointInside(Point3d point, Autodesk.AutoCAD.DatabaseServices.Polyline pline)
        {
            double tolerance = Tolerance.Global.EqualPoint;
            using (MPolygon mpg = new MPolygon())
            {
                mpg.AppendLoopFromBoundary(pline, true, 0);
                return mpg.IsPointInsideMPolygon(point, 0).Count == 1;
            }
        }
        private void CemberYaz(Autodesk.AutoCAD.DatabaseServices.Polyline pline, List<Point3d> list, double yaricap)
        {
            Document acDoc = Application.DocumentManager.MdiActiveDocument;
            Database acCurDb = acDoc.Database;
            using (Transaction acTrans = acCurDb.TransactionManager.StartTransaction())
            {
                BlockTable acBlkTbl;
                acBlkTbl = acTrans.GetObject(acCurDb.BlockTableId, OpenMode.ForRead) as BlockTable;
                BlockTableRecord acBlkTblRec;
                acBlkTblRec = acTrans.GetObject(acBlkTbl[BlockTableRecord.ModelSpace], OpenMode.ForWrite) as BlockTableRecord;
                foreach (var item in list)
                {
                    if (IsPointInside(new Point3d(item.X - yaricap + (yaricap / 2), item.Y, 0), pline) == true && IsPointInside(new Point3d(item.X, item.Y - yaricap + (yaricap / 2), 0), pline) == true && IsPointInside(new Point3d(item.X + yaricap - (yaricap / 2), item.Y, 0), pline) == true && IsPointInside(new Point3d(item.X, item.Y + yaricap - (yaricap / 2), 0), pline) == true)
                    {
                        using (Circle acCirc = new Circle())
                        {
                            kesismenoktalari.Add(item);
                            acCirc.Center = item;
                            acCirc.Diameter = (cembercap > 0) ? cembercap : 30;
                            //acCirc.Color = Color.FromColorIndex(ColorMethod.ByBlock, 3);
                            acBlkTblRec.AppendEntity(acCirc);
                            acTrans.AddNewlyCreatedDBObject(acCirc, true);
                            en.centerpoint = item;
                            en.objectid = acCurDb.BlockTableId;
                            entitylist.Add(en);
                        }

                    }
                }
                acTrans.Commit();
            }

        }
        private void AksCemberleri(Point3d startpoint, double yatayaksaraligi, double dikeyaksaraligi, Autodesk.AutoCAD.DatabaseServices.Polyline pline, double yaricap)
        {
            soldangirinti = 0;
            yukaridanGirinti = 0;
            cemberListesi = new List<Point3d>();
            List<Point3d> asagiInennokta;
            Point3d kosulnoktasi = startpoint;
            bool asagi = true;
            bool sag = true;
            globalSolagidilen = new List<int>();
            d_c_sayisi = 0;
            global_y_c_sayisi = new List<int>();
            Point3d firstpoint = new Point3d();
            soldangirinti = GirintiBelirle(startpoint, pline, dikeyaksaraligi, yaricap);
            yukaridanGirinti = AsagiGirintiBelirle(startpoint, pline, yatayaksaraligi, yaricap);
            startpoint = new Point3d(startpoint.X + soldangirinti, startpoint.Y - yukaridanGirinti, startpoint.Z);
            ybaslanıcnoktasi = startpoint.Y;
            while (sag || asagi)
            {
                solagidilen = 0;
                asagiInennokta = new List<Point3d>();
                yerel_y_c_sayisi = 0;
                //startpoint = new Point3d(startpoint.X, startpoint.Y, startpoint.Z);
                if (IsPointInside(new Point3d(startpoint.X, startpoint.Y, 0), pline))
                {                  
                    cemberListesi.Add(startpoint);
                    d_c_sayisi++;
                    yerel_y_c_sayisi++;
                    firstpoint = startpoint;
                    asagiInennokta.Add(startpoint);
                    ynoktalari.Add(startpoint.Y);
                    xnoktalari.Add(startpoint.X);
                }
                if (!IsPointInside(new Point3d(startpoint.X, startpoint.Y - yatayaksaraligi, startpoint.Z), pline))//yarıçap eklenme durumu incelenecek.
                {
                    //firstpoint = startpoint;
                    while (IsPointInside(new Point3d(startpoint.X + dikeyaksaraligi, startpoint.Y, startpoint.Z), pline))
                    {
                        sag = true;
                        yerel_y_c_sayisi++;
                        cemberListesi.Add(new Point3d(startpoint.X + dikeyaksaraligi, startpoint.Y, startpoint.Z));
                        xnoktalari.Add(startpoint.X + dikeyaksaraligi);
                        startpoint = new Point3d(startpoint.X + dikeyaksaraligi, startpoint.Y, startpoint.Z);
                        asagiInennokta.Add(startpoint);
                    }
                    sag = false;
                    startpoint = firstpoint;
                    while (IsPointInside(new Point3d(startpoint.X - dikeyaksaraligi, startpoint.Y, startpoint.Z), pline))
                    {
                        //solagidilen++;
                        yerel_y_c_sayisi++;
                        cemberListesi.Add(new Point3d(startpoint.X - dikeyaksaraligi, startpoint.Y, startpoint.Z));
                        xnoktalari.Add(startpoint.X - dikeyaksaraligi);
                        startpoint = new Point3d(startpoint.X - dikeyaksaraligi, startpoint.Y, startpoint.Z);
                        if (startpoint.X < kosulnoktasi.X)
                        {
                            solagidilen++;
                        }

                        asagiInennokta.Add(startpoint);
                    }
                    startpoint = firstpoint; // en son eklenen

                    if (KalinanSonNokta(asagiInennokta, yatayaksaraligi, pline) == startpoint)
                    {
                        asagi = false; sag = false;
                        global_y_c_sayisi.Add(yerel_y_c_sayisi);
                        globalSolagidilen.Add(solagidilen);
                        break;
                    }
                    else
                    {
                        Point3d p = KalinanSonNokta(asagiInennokta, yatayaksaraligi, pline);
                        startpoint = new Point3d(p.X, p.Y - yatayaksaraligi, p.Z);
                    }
                    global_y_c_sayisi.Add(yerel_y_c_sayisi);
                    globalSolagidilen.Add(solagidilen);
                }
                else
                {
                    while (IsPointInside(new Point3d(startpoint.X + dikeyaksaraligi, startpoint.Y, startpoint.Z), pline))
                    {
                        sag = true;
                        yerel_y_c_sayisi++;
                        cemberListesi.Add(new Point3d(startpoint.X + dikeyaksaraligi, startpoint.Y, startpoint.Z));
                        xnoktalari.Add(startpoint.X + dikeyaksaraligi);
                        startpoint = new Point3d(startpoint.X + dikeyaksaraligi, startpoint.Y, startpoint.Z);

                    }
                    sag = false;
                    startpoint = firstpoint;
                    while (IsPointInside(new Point3d(startpoint.X - dikeyaksaraligi, startpoint.Y, startpoint.Z), pline))
                    {
                        //solagidilen++;
                        yerel_y_c_sayisi++;
                        cemberListesi.Add(new Point3d(startpoint.X - dikeyaksaraligi, startpoint.Y, startpoint.Z));
                        xnoktalari.Add(startpoint.X - dikeyaksaraligi);
                        startpoint = new Point3d(startpoint.X - dikeyaksaraligi, startpoint.Y, startpoint.Z);
                        if (startpoint.X < kosulnoktasi.X)
                        {
                            solagidilen++;
                        }
                        solagidilmedurumu = true;
                    }
                    startpoint = firstpoint;
                    startpoint = new Point3d(startpoint.X, startpoint.Y - yatayaksaraligi, startpoint.Z);
                    global_y_c_sayisi.Add(yerel_y_c_sayisi);
                    globalSolagidilen.Add(solagidilen);
                }
            }
            yerel_y_c_sayisi = global_y_c_sayisi.Max();
            solagidilen = globalSolagidilen.Max();
            yukaricemberler = new List<Point3d>();
            List<Point3d> ilksatir = cemberListesi.Where(x => x.Y == ybaslanıcnoktasi).ToList();
            foreach (var item in ilksatir)
            {

                Point3d p = CemberEkle(pline, item, yatayaksaraligi);
                for (double i = item.Y + yatayaksaraligi; i <= p.Y; i += yatayaksaraligi)
                {
                    yukaricemberler.Add(new Point3d(item.X, i, item.Z));
                    xnoktalari.Add(item.X);
                    ynoktalari.Add(i);
                }

            }
            ilkCember = cemberListesi.First();
            cemberListesi.AddRange(yukaricemberler);
            cemberListesi = cemberListesi.OrderByDescending(p => p.Y).ThenBy(p => p.X).ToList();
            CemberYaz(pline, cemberListesi, yaricap);
        }
        private Point3d CemberEkle(Autodesk.AutoCAD.DatabaseServices.Polyline pline, Point3d point, double yatayaralik)
        {
            Point3d p = new Point3d(point.X, point.Y, point.Z);
            while (IsPointInside(new Point3d(p.X, p.Y + yatayaralik, p.Z), pline))
            {
                p = new Point3d(point.X, point.Y + yatayaralik, point.Z);
                if (IsPointInside(new Point3d(p.X, p.Y + yatayaralik, p.Z), pline)) p = new Point3d(p.X, p.Y + yatayaralik, p.Z);
                else break;
            }
            return p;

        }
        private void AsagıGirintiDuzenle(Autodesk.AutoCAD.DatabaseServices.Polyline pline)
        {
            List<Point3d> cemberList = new List<Point3d>();
            Point3d sonCember = cemberListesi.LastOrDefault();
            double uzunluk = 0;

            cemberList = cemberListesi;
            while (IsPointInside(sonCember, pline))
            {
                uzunluk++;
                sonCember = sonCember = new Point3d(sonCember.X, sonCember.Y - 1, sonCember.Z);
            }
            double girinti = uzunluk / 2;
            foreach (var item in cemberListesi)
            {
                cemberList.Add(new Point3d(item.X, item.Y - girinti, item.Z));
            }
            cemberListesi = cemberList;
        }
        private double GirintiBelirle(Point3d startpoint, Autodesk.AutoCAD.DatabaseServices.Polyline pline, double dikeyaralik, double yaricap)
        {
            Point3d firstPoint = startpoint;
            double girinti = 0;
            double uzunluk = 0;
            while (IsPointInside(new Point3d(startpoint.X + dikeyaralik + yaricap, startpoint.Y, startpoint.Z), pline))
            {
                uzunluk += dikeyaralik + yaricap;
                startpoint = new Point3d(startpoint.X + dikeyaralik + yaricap, startpoint.Y, startpoint.Z);
            }
            while (IsPointInside(new Point3d(startpoint.X + 1, startpoint.Y, startpoint.Z), pline))
            {
                uzunluk++;
                startpoint = new Point3d(startpoint.X + 1, startpoint.Y, startpoint.Z);
            }
            uzunluk -= yaricap;
            girinti = (uzunluk % dikeyaralik) / 2;

            return girinti;
        }
        private double AsagiGirintiBelirle(Point3d startpoint, Autodesk.AutoCAD.DatabaseServices.Polyline pline, double yatayaralik, double yaricap)
        {
            Point3d firstPoint = startpoint;
            double girinti = 0;
            double uzunluk = 0;
            //if (IsPointInside(new Point3d(startpoint.X, startpoint.Y - 1, startpoint.Z), pline))
            //{
            //    while (IsPointInside(startpoint, pline))
            //    {
            //        uzunluk++;
            //        startpoint = new Point3d(startpoint.X, startpoint.Y - 1, startpoint.Z);
            //    }
            //}
            //uzunluk -= bitisdegeri;
            while (IsPointInside(new Point3d(startpoint.X, startpoint.Y - yatayaralik - yaricap, startpoint.Z), pline))
            {
                uzunluk += yatayaralik + yaricap;
                startpoint = new Point3d(startpoint.X, startpoint.Y - yatayaralik - yaricap, startpoint.Z);
            }
            while (IsPointInside(new Point3d(startpoint.X, startpoint.Y - 1, startpoint.Z), pline))
            {
                uzunluk++;
                startpoint = new Point3d(startpoint.X, startpoint.Y - 1, startpoint.Z);
            }
            uzunluk -= yaricap;
            girinti = (uzunluk % yatayaralik) / 2;
            return girinti;
        }
        private Point3d KalinanSonNokta(List<Point3d> list, double yatayaralik, Autodesk.AutoCAD.DatabaseServices.Polyline pline)
        {
            foreach (var nokta in list)
            {
                if (IsPointInside(new Point3d(nokta.X, nokta.Y - yatayaralik, nokta.Z), pline)) return nokta;
            }
            return list.First();
        }
        private void Yatay_Aks_Cizimi(double yatayaksaraligi, double dikeyaksaraligi, Point3d startpoint, Autodesk.AutoCAD.DatabaseServices.Polyline pline, double xmax, int cembersayisi)
        {
            if (solagidilen > 0)
            {
                for (int i = 0; i < solagidilen; i++)
                {
                    startpoint = new Point3d(startpoint.X - dikeyaksaraligi, startpoint.Y, startpoint.Z);
                }
            }
            Document acDoc = Application.DocumentManager.MdiActiveDocument;
            Database acCurDb = acDoc.Database;
            using (Transaction acTrans = acCurDb.TransactionManager.StartTransaction())
            {
                Point3d point = new Point3d(startpoint.X, startpoint.Y, startpoint.Z);
                BlockTable acBlkTbl;
                acBlkTbl = acTrans.GetObject(acCurDb.BlockTableId, OpenMode.ForRead) as BlockTable;
                BlockTableRecord acBlkTblRec;
                acBlkTblRec = acTrans.GetObject(acBlkTbl[BlockTableRecord.ModelSpace], OpenMode.ForWrite) as BlockTableRecord;

                for (int i = 0; i < cembersayisi; i++)
                {
                    Point3d startline = new Point3d(point.X - 800 - soldangirinti, point.Y, point.Z);
                    Point3d endline = new Point3d(xmax + 800 + soldangirinti, point.Y, point.Z);
                    Line line = new Line(startline, endline);
                    line.Color = Color.FromColorIndex(ColorMethod.ByBlock, 4);
                    acBlkTblRec.AppendEntity(line);
                    acTrans.AddNewlyCreatedDBObject(line, true);
                    point = new Point3d(point.X, point.Y - yatayaksaraligi, point.Z);
                    globalyatayakssayisi++;
                    Yaksnoktalaribaslangic.Add(startline);
                    Yaksnoktalaribitis.Add(endline);
                }
                acTrans.Commit();
            }
        }
        private void Dikey_Aks_Cizimi(double dikeyaksaraligi, double yatayaksaraligi, Point3d startpoint, Autodesk.AutoCAD.DatabaseServices.Polyline pline, double ymin, int cembersayisi)
        {
            if (solagidilen > 0)
            {
                for (int i = 0; i < solagidilen; i++)
                {
                    startpoint = new Point3d(startpoint.X - dikeyaksaraligi, startpoint.Y, startpoint.Z);
                }
            }
            Document acDoc = Application.DocumentManager.MdiActiveDocument;
            Database acCurDb = acDoc.Database;
            using (Transaction acTrans = acCurDb.TransactionManager.StartTransaction())
            {
                Point3d point = new Point3d(startpoint.X, startpoint.Y, startpoint.Z);
                BlockTable acBlkTbl;
                acBlkTbl = acTrans.GetObject(acCurDb.BlockTableId, OpenMode.ForRead) as BlockTable;
                BlockTableRecord acBlkTblRec;
                acBlkTblRec = acTrans.GetObject(acBlkTbl[BlockTableRecord.ModelSpace], OpenMode.ForWrite) as BlockTableRecord;

                for (int i = 0; i < cembersayisi; i++)
                {
                    Point3d startline = new Point3d(point.X, point.Y + 800 + yukaridanGirinti, point.Z);
                    Point3d endline = new Point3d(point.X, ymin - 800 - yukaridanGirinti, point.Z);
                    Line line = new Line(startline, endline);
                    line.Color = Color.FromColorIndex(ColorMethod.ByBlock, 4);
                    acBlkTblRec.AppendEntity(line);
                    acTrans.AddNewlyCreatedDBObject(line, true);
                    point = new Point3d(point.X + dikeyaksaraligi, point.Y, point.Z);
                    Xaksnoktalaribaslangic.Add(startline);
                    Xaksnoktalaribitis.Add(endline);
                    globaldikeyakssayisi++;
                }
                acTrans.Commit();
            }
        }
        private void DikeyAks(double dikeyaksaraligi, Point3d startpoint, double ymin)
        {
            Document acDoc = Application.DocumentManager.MdiActiveDocument;
            Database acCurDb = acDoc.Database;
            using (Transaction acTrans = acCurDb.TransactionManager.StartTransaction())
            {
                BlockTable acBlkTbl;
                acBlkTbl = acTrans.GetObject(acCurDb.BlockTableId, OpenMode.ForRead) as BlockTable;
                BlockTableRecord acBlkTblRec;
                acBlkTblRec = acTrans.GetObject(acBlkTbl[BlockTableRecord.ModelSpace], OpenMode.ForWrite) as BlockTableRecord;
                double y = ynoktalari.Max();
                double xbaslangic = xnoktalari.Min();
                double xbitis = xnoktalari.Max();
                Point3d point = new Point3d(xbaslangic, y, 0);
                for (double i = xbaslangic; i <= xbitis; i += dikeyaksaraligi)
                {
                    Point3d startline = new Point3d(i, point.Y + 800 + yukaridanGirinti, point.Z);
                    Point3d endline = new Point3d(i, ymin - 800 - yukaridanGirinti, point.Z);
                    Line line = new Line(startline, endline);
                    line.Color = Color.FromColorIndex(ColorMethod.ByBlock, 4);
                    acBlkTblRec.AppendEntity(line);
                    acTrans.AddNewlyCreatedDBObject(line, true);
                    point = new Point3d(point.X + dikeyaksaraligi, point.Y, point.Z);
                    Xaksnoktalaribaslangic.Add(startline);
                    Xaksnoktalaribitis.Add(endline);
                }
                acTrans.Commit();
            }
            ynoktalari.Clear();
            xnoktalari.Clear();
        }
        private void YatayAks(double yatayaksaraligi, Point3d startpoint, double xmax)
        {
            Document acDoc = Application.DocumentManager.MdiActiveDocument;
            Database acCurDb = acDoc.Database;
            using (Transaction acTrans = acCurDb.TransactionManager.StartTransaction())
            {
                BlockTable acBlkTbl;
                acBlkTbl = acTrans.GetObject(acCurDb.BlockTableId, OpenMode.ForRead) as BlockTable;
                BlockTableRecord acBlkTblRec;
                acBlkTblRec = acTrans.GetObject(acBlkTbl[BlockTableRecord.ModelSpace], OpenMode.ForWrite) as BlockTableRecord;
                double x = xnoktalari.Min();
                double ybaslangic = ynoktalari.Max();
                double ybitis = ynoktalari.Min();
                Point3d point = new Point3d(x, ybaslangic, 0);

                for (double i = ybaslangic; i >= ybitis; i -= yatayaksaraligi)
                {
                    Point3d startline = new Point3d(point.X - 800 - soldangirinti, i, point.Z);
                    Point3d endline = new Point3d(xmax + 800 + soldangirinti, i, point.Z);
                    Line line = new Line(startline, endline);
                    line.Color = Color.FromColorIndex(ColorMethod.ByBlock, 4);
                    acBlkTblRec.AppendEntity(line);
                    acTrans.AddNewlyCreatedDBObject(line, true);
                    point = new Point3d(point.X, point.Y - yatayaksaraligi, point.Z);
                    globalyatayakssayisi++;
                    Yaksnoktalaribaslangic.Add(startline);
                    Yaksnoktalaribitis.Add(endline);
                }
                acTrans.Commit();
            }
        }
        private void YaziOlustur(List<Point3d> list, string text)
        {
            int i = 1;
            Document acDoc = Application.DocumentManager.MdiActiveDocument;
            Database acCurDb = acDoc.Database;
            using (Transaction acTrans = acCurDb.TransactionManager.StartTransaction())
            {
                BlockTable acBlkTbl;
                acBlkTbl = acTrans.GetObject(acCurDb.BlockTableId, OpenMode.ForRead) as BlockTable;
                BlockTableRecord acBlkTblRec;
                acBlkTblRec = acTrans.GetObject(acBlkTbl[BlockTableRecord.ModelSpace], OpenMode.ForWrite) as BlockTableRecord;
                foreach (var item in list)
                {
                    using (DBText acText = new DBText())
                    {
                        acText.Position = item;
                        acText.Height = 20;
                        acText.TextString = text + "" + i.ToString();

                        acBlkTblRec.AppendEntity(acText);
                        acTrans.AddNewlyCreatedDBObject(acText, true);
                    }
                    i++;
                }

                acTrans.Commit();
            }
        }
        private void XaksOlcuOlustur(List<Point3d> list, double dikeyaralik)//aks aralarındaki ölçüler
        {

            Document acDoc = Application.DocumentManager.MdiActiveDocument;
            Database acCurDb = acDoc.Database;
            using (Transaction acTrans = acCurDb.TransactionManager.StartTransaction())
            {
                BlockTable acBlkTbl;
                acBlkTbl = acTrans.GetObject(acCurDb.BlockTableId, OpenMode.ForRead) as BlockTable;
                BlockTableRecord acBlkTblRec;
                acBlkTblRec = acTrans.GetObject(acBlkTbl[BlockTableRecord.ModelSpace], OpenMode.ForWrite) as BlockTableRecord;
                using (DBText acText = new DBText())
                {
                    acText.Height = 50;
                    acText.TextString = (dikeyaralik * (list.Count() - 1)).ToString();
                    if (durumx)
                    {
                        for (int i = 0; i < list.Count - 1; i++)
                        {
                            using (DBText acText2 = new DBText())
                            {
                                acText2.Height = 50;

                                acText2.TextString = dikeyaralik.ToString();
                                double nokta = list[i].X + ((list[i + 1].X - list[i].X) - (acText2.Bounds.Value.MaxPoint.X - acText2.Bounds.Value.MinPoint.X)) / 2;
                                acText2.Position = new Point3d(nokta, list[i].Y - 300, list[i].Z);
                                acBlkTblRec.AppendEntity(acText2);
                                acTrans.AddNewlyCreatedDBObject(acText2, true);
                                Line line = new Line(new Point3d(list[i].X, list[i].Y - 275, list[i].Z), new Point3d(acText2.Bounds.Value.MinPoint.X, list[i].Y - 275, list[i].Z));
                                acBlkTblRec.AppendEntity(line);
                                acTrans.AddNewlyCreatedDBObject(line, true);
                                Line line2 = new Line(new Point3d(acText2.Bounds.Value.MaxPoint.X, list[i + 1].Y - 275, list[i + 1].Z), new Point3d(list[i + 1].X, list[i + 1].Y - 275, list[i + 1].Z));
                                acBlkTblRec.AppendEntity(line2);
                                acTrans.AddNewlyCreatedDBObject(line2, true);
                            }
                        }
                        // toplam uzunluğun gösterilmesi üst kısım

                        acText.Position = new Point3d(list.First().X + ((list.Last().X - list.First().X) - acText.Bounds.Value.MaxPoint.X - acText.Bounds.Value.MinPoint.X) / 2, list.First().Y - 200, 0);
                        Line l = new Line(new Point3d(list.First().X, list.First().Y - 175, list.First().Z), new Point3d(acText.Bounds.Value.MinPoint.X, list.First().Y - 175, list.First().Z));
                        acBlkTblRec.AppendEntity(l);
                        acTrans.AddNewlyCreatedDBObject(l, true);
                        Line l2 = new Line(new Point3d(acText.Bounds.Value.MaxPoint.X, list.First().Y - 175, list.First().Z), new Point3d(list.Last().X, list.Last().Y - 175, list.Last().Z));
                        acBlkTblRec.AppendEntity(l2);
                        acTrans.AddNewlyCreatedDBObject(l2, true);
                        durumx = false;
                    }
                    else
                    {

                        for (int i = 0; i < list.Count - 1; i++)
                        {
                            using (DBText acText2 = new DBText())
                            {
                                acText2.Height = 50;
                                acText2.TextString = dikeyaralik.ToString();
                                double nokta = list[i].X + ((list[i + 1].X - list[i].X) - (acText2.Bounds.Value.MaxPoint.X - acText2.Bounds.Value.MinPoint.X)) / 2;
                                acText2.Position = new Point3d(nokta, list[i].Y + 300, list[i].Z);
                                acBlkTblRec.AppendEntity(acText2);
                                acTrans.AddNewlyCreatedDBObject(acText2, true);

                                Line line = new Line(new Point3d(list[i].X, list[i].Y + 325, list[i].Z), new Point3d(acText2.Bounds.Value.MinPoint.X, list[i].Y + 325, list[i].Z));
                                acBlkTblRec.AppendEntity(line);
                                acTrans.AddNewlyCreatedDBObject(line, true);
                                Line line2 = new Line(new Point3d(acText2.Bounds.Value.MaxPoint.X, list[i + 1].Y + 325, list[i + 1].Z), new Point3d(list[i + 1].X, list[i + 1].Y + 325, list[i + 1].Z));
                                acBlkTblRec.AppendEntity(line2);
                                acTrans.AddNewlyCreatedDBObject(line2, true);
                            }
                        }
                        //Toplam uzunluğun gösterilmesi alt kısım
                        acText.Position = new Point3d(list.First().X + ((list.Last().X - list.First().X) - acText.Bounds.Value.MaxPoint.X - acText.Bounds.Value.MinPoint.X) / 2, list.First().Y + 200, 0);
                        Line l = new Line(new Point3d(list.First().X, list.First().Y + 225, list.First().Z), new Point3d(acText.Bounds.Value.MinPoint.X, list.First().Y + 225, list.First().Z));
                        acBlkTblRec.AppendEntity(l);
                        acTrans.AddNewlyCreatedDBObject(l, true);
                        Line l2 = new Line(new Point3d(acText.Bounds.Value.MaxPoint.X, list.First().Y + 225, list.First().Z), new Point3d(list.Last().X, list.Last().Y + 225, list.Last().Z));
                        acBlkTblRec.AppendEntity(l2);
                        acTrans.AddNewlyCreatedDBObject(l2, true);
                        durumx = true;
                    }

                    acBlkTblRec.AppendEntity(acText);
                    acTrans.AddNewlyCreatedDBObject(acText, true);
                }
                acTrans.Commit();
            }
        }
        private void YaksOlcuOlustur(List<Point3d> list, double yatayaralik)
        {
            Document acDoc = Application.DocumentManager.MdiActiveDocument;
            Database acCurDb = acDoc.Database;
            using (Transaction acTrans = acCurDb.TransactionManager.StartTransaction())
            {
                BlockTable acBlkTbl;
                acBlkTbl = acTrans.GetObject(acCurDb.BlockTableId, OpenMode.ForRead) as BlockTable;
                BlockTableRecord acBlkTblRec;
                acBlkTblRec = acTrans.GetObject(acBlkTbl[BlockTableRecord.ModelSpace], OpenMode.ForWrite) as BlockTableRecord;
                using (DBText acText = new DBText())
                {
                    acText.Height = 50;
                    acText.Rotation = 33;
                    acText.TextString = (yatayaralik * (list.Count() - 1)).ToString();
                    if (durumy)
                    {
                        for (int i = 0; i < list.Count - 1; i++)
                        {
                            using (DBText acText2 = new DBText())
                            {
                                acText2.Height = 50;
                                acText2.Rotation = 33;
                                acText2.TextString = yatayaralik.ToString();
                                double nokta = list[i + 1].Y + ((list[i].Y - list[i + 1].Y) - (acText2.Bounds.Value.MaxPoint.Y - acText2.Bounds.Value.MinPoint.Y)) / 2;
                                acText2.Position = new Point3d(list[i].X + 300, nokta, list[i].Z);
                                acBlkTblRec.AppendEntity(acText2);
                                acTrans.AddNewlyCreatedDBObject(acText2, true);
                                Line line = new Line(new Point3d(list[i].X + 275, list[i].Y, list[i].Z), new Point3d(list[i].X + 275, acText2.Bounds.Value.MaxPoint.Y, list[i].Z));
                                acBlkTblRec.AppendEntity(line);
                                acTrans.AddNewlyCreatedDBObject(line, true);
                                Line line2 = new Line(new Point3d(list[i + 1].X + 275, acText2.Bounds.Value.MinPoint.Y, list[i + 1].Z), new Point3d(list[i + 1].X + 275, list[i + 1].Y, list[i + 1].Z));
                                acBlkTblRec.AppendEntity(line2);
                                acTrans.AddNewlyCreatedDBObject(line2, true);
                            }
                        }
                        //                       
                        acText.Position = new Point3d(list.First().X + 200, list.Last().Y + ((list.First().Y - list.Last().Y) - acText.Bounds.Value.MaxPoint.Y - acText.Bounds.Value.MinPoint.Y) / 2, list.First().Z);
                        Line l = new Line(new Point3d(list.First().X + 175, list.First().Y, list.First().Z), new Point3d(list.First().X + 175, acText.Bounds.Value.MaxPoint.Y, list.First().Z));
                        acBlkTblRec.AppendEntity(l);
                        acTrans.AddNewlyCreatedDBObject(l, true);
                        Line l2 = new Line(new Point3d(list.First().X + 175, acText.Bounds.Value.MinPoint.Y, list.First().Z), new Point3d(list.Last().X + 175, list.Last().Y, list.Last().Z));
                        acBlkTblRec.AppendEntity(l2);
                        acTrans.AddNewlyCreatedDBObject(l2, true);

                        durumy = false;
                    }
                    else
                    {
                        for (int i = 0; i < list.Count - 1; i++)
                        {
                            using (DBText acText2 = new DBText())
                            {
                                acText2.Height = 50;
                                acText2.TextString = yatayaralik.ToString();
                                acText2.Rotation = 33;
                                double nokta = list[i + 1].Y + ((list[i].Y - list[i + 1].Y) - (acText2.Bounds.Value.MaxPoint.Y - acText2.Bounds.Value.MinPoint.Y)) / 2;
                                acText2.Position = new Point3d(list[i].X - 300, nokta, list[i].Z);
                                acBlkTblRec.AppendEntity(acText2);
                                acTrans.AddNewlyCreatedDBObject(acText2, true);
                                Line line = new Line(new Point3d(list[i].X - 325, list[i].Y, list[i].Z), new Point3d(list[i].X - 325, acText2.Bounds.Value.MaxPoint.Y, list[i].Z));
                                acBlkTblRec.AppendEntity(line);
                                acTrans.AddNewlyCreatedDBObject(line, true);
                                Line line2 = new Line(new Point3d(list[i + 1].X - 325, acText2.Bounds.Value.MinPoint.Y, list[i + 1].Z), new Point3d(list[i + 1].X - 325, list[i + 1].Y, list[i + 1].Z));
                                acBlkTblRec.AppendEntity(line2);
                                acTrans.AddNewlyCreatedDBObject(line2, true);
                            }
                        }
                        //
                        acText.Position = new Point3d(list.First().X - 200, list.Last().Y + ((list.First().Y - list.Last().Y) - acText.Bounds.Value.MaxPoint.Y - acText.Bounds.Value.MinPoint.Y) / 2, list.First().Z);
                        Line l = new Line(new Point3d(list.First().X - 225, list.First().Y, list.First().Z), new Point3d(list.First().X - 225, acText.Bounds.Value.MaxPoint.Y, list.First().Z));
                        acBlkTblRec.AppendEntity(l);
                        acTrans.AddNewlyCreatedDBObject(l, true);
                        Line l2 = new Line(new Point3d(list.First().X - 225, acText.Bounds.Value.MinPoint.Y, list.First().Z), new Point3d(list.Last().X - 225, list.Last().Y, list.Last().Z));
                        acBlkTblRec.AppendEntity(l2);
                        acTrans.AddNewlyCreatedDBObject(l2, true);
                        durumy = true;
                    }
                    acBlkTblRec.AppendEntity(acText);
                    acTrans.AddNewlyCreatedDBObject(acText, true);
                }
                acTrans.Commit();
            }
        }
        private List<Point3d> NoktalariGüncelle(List<Point3d> list, double x, double y)
        {
            List<Point3d> newlist = new List<Point3d>();
            foreach (var item in list)
            {
                newlist.Add(new Point3d(item.X + x, item.Y - y, item.Z));
            }
            return newlist;
        }
        private List<Point3d> NoktalariGüncelle2(List<Point3d> list, double x, double y)
        {
            List<Point3d> newlist = new List<Point3d>();
            foreach (var item in list)
            {
                newlist.Add(new Point3d(item.X - x, item.Y + y, item.Z));
            }

            return newlist;

        }
        private void articiz(Point3d pointcenter, double ymin, double ymax, double xmin, double xmax)
        {
            Document acDoc = Application.DocumentManager.MdiActiveDocument;
            Database acCurDb = acDoc.Database;
            using (Transaction acTrans = acCurDb.TransactionManager.StartTransaction())
            {

                BlockTable acBlkTbl;
                acBlkTbl = acTrans.GetObject(acCurDb.BlockTableId, OpenMode.ForRead) as BlockTable;
                BlockTableRecord acBlkTblRec;
                acBlkTblRec = acTrans.GetObject(acBlkTbl[BlockTableRecord.ModelSpace], OpenMode.ForWrite) as BlockTableRecord;

                Point3d startline1 = pointcenter;
                Point3d endline1 = new Point3d(pointcenter.X, ymax + 200, 0);
                Line line1 = new Line(startline1, endline1);

                Point3d startline2 = pointcenter;
                Point3d endline2 = new Point3d(xmax + 200, pointcenter.Y, 0);
                Line line2 = new Line(startline2, endline2);

                Point3d startline3 = pointcenter;
                Point3d endline3 = new Point3d(pointcenter.X, ymin - 200, 0);
                Line line3 = new Line(startline3, endline3);

                Point3d startline4 = pointcenter;
                Point3d endline4 = new Point3d(xmin - 200, pointcenter.Y, 0);
                Line line4 = new Line(startline4, endline4);

                acBlkTblRec.AppendEntity(line1);
                acBlkTblRec.AppendEntity(line2);
                acBlkTblRec.AppendEntity(line3);
                acBlkTblRec.AppendEntity(line4);

                acTrans.AddNewlyCreatedDBObject(line1, true);
                acTrans.AddNewlyCreatedDBObject(line2, true);
                acTrans.AddNewlyCreatedDBObject(line3, true);
                acTrans.AddNewlyCreatedDBObject(line4, true);

                en.objectid = line1.ObjectId;
                en.centerpoint = Metods.MerkezNokta(line1.ObjectId);
                entitylist.Add(en);


                en.objectid = line2.ObjectId;
                en.centerpoint = Metods.MerkezNokta(line2.ObjectId);
                entitylist.Add(en);


                en.objectid = line3.ObjectId;
                en.centerpoint = Metods.MerkezNokta(line3.ObjectId);
                entitylist.Add(en);


                en.objectid = line4.ObjectId;
                en.centerpoint = Metods.MerkezNokta(line4.ObjectId);
                entitylist.Add(en);

                #region yazilar
                using (DBText acText1 = new DBText())
                {
                    acText1.Position = new Point3d(endline1.X - 20, ymax + 180, 0);
                    acText1.Height = 70;
                    acText1.Rotation = 33;
                    acText1.TextString = "B";
                    acBlkTblRec.AppendEntity(acText1);
                    acTrans.AddNewlyCreatedDBObject(acText1, true);
                }
                using (DBText acText3 = new DBText())
                {
                    acText3.Position = new Point3d(endline3.X - 20, ymin - 190, 0);
                    acText3.Height = 70;
                    acText3.Rotation = 33;
                    acText3.TextString = "B";
                    acBlkTblRec.AppendEntity(acText3);
                    acTrans.AddNewlyCreatedDBObject(acText3, true);
                }
                using (DBText acText2 = new DBText())
                {
                    acText2.Position = new Point3d(xmax + 180, endline2.Y + 20, 0);
                    acText2.Height = 70;
                    acText2.TextString = "A";
                    acBlkTblRec.AppendEntity(acText2);
                    acTrans.AddNewlyCreatedDBObject(acText2, true);
                }
                using (DBText acText2 = new DBText())
                {
                    acText2.Position = new Point3d(xmin - 190, endline4.Y + 20, 0);
                    acText2.Height = 70;
                    acText2.TextString = "A";
                    acBlkTblRec.AppendEntity(acText2);
                    acTrans.AddNewlyCreatedDBObject(acText2, true);
                }
                #endregion
                acTrans.Commit();
            }
        }

        private void GirintileriYazdir(double yaricap, Autodesk.AutoCAD.DatabaseServices.Polyline pline)
        {
            soldangirinti = 0; yukaridanGirinti = 0;
            Point3d spoint = ilkCember;
            while (IsPointInside(new Point3d(spoint.X, spoint.Y + 1, spoint.Z), pline))
            {
                yukaridanGirinti++;
                spoint = new Point3d(spoint.X, spoint.Y + 1, spoint.Z);
            }
            spoint = ilkCember;
            while (IsPointInside(new Point3d(spoint.X - 1, spoint.Y, spoint.Z), pline))
            {
                soldangirinti++;
                spoint = new Point3d(spoint.X - 1, spoint.Y, spoint.Z);
            }
            Document acDoc = Application.DocumentManager.MdiActiveDocument;
            Database acCurDb = acDoc.Database;
            using (Transaction acTrans = acCurDb.TransactionManager.StartTransaction())
            {

                BlockTable acBlkTbl;
                acBlkTbl = acTrans.GetObject(acCurDb.BlockTableId, OpenMode.ForRead) as BlockTable;
                BlockTableRecord acBlkTblRec;
                acBlkTblRec = acTrans.GetObject(acBlkTbl[BlockTableRecord.ModelSpace], OpenMode.ForWrite) as BlockTableRecord;

                Point3d startline1 = new Point3d(ilkCember.X, ilkCember.Y-(yaricap / 2), ilkCember.Z);
                Point3d endline1 = new Point3d(ilkCember.X-soldangirinti, ilkCember.Y - (yaricap / 2), 0);
                Line line1 = new Line(startline1, endline1);
                line1.Color = Color.FromColorIndex(ColorMethod.ByBlock, 1);
                Point3d startline2 = new Point3d(ilkCember.X + (yaricap / 2), ilkCember.Y, ilkCember.Z);
                Point3d endline2 = new Point3d(ilkCember.X+(yaricap/2), ilkCember.Y + yukaridanGirinti , ilkCember.Z);
                Line line2 = new Line(startline2, endline2);
                line2.Color = Color.FromColorIndex(ColorMethod.ByBlock, 1);

                acBlkTblRec.AppendEntity(line1);
                acBlkTblRec.AppendEntity(line2);

                acTrans.AddNewlyCreatedDBObject(line1, true);
                acTrans.AddNewlyCreatedDBObject(line2, true);

                Point3d yatay = Metods.MerkezNokta(line1.ObjectId);
                Point3d dikey = Metods.MerkezNokta(line2.ObjectId);

                #region yazilar
                using (DBText acText1 = new DBText())
                {
                    acText1.Position = new Point3d(yatay.X,yatay.Y-(yaricap/4),0);
                    acText1.Height = yaricap/4;
                    acText1.TextString = soldangirinti.ToString();
                    acBlkTblRec.AppendEntity(acText1);
                    acTrans.AddNewlyCreatedDBObject(acText1, true);
                }
                using (DBText acText3 = new DBText())
                {
                    acText3.Position = new Point3d(dikey.X+(yaricap/4),dikey.Y,0);
                    acText3.Height = yaricap/4;
                    acText3.Rotation = 33;
                    acText3.TextString = yukaridanGirinti.ToString();
                    acBlkTblRec.AppendEntity(acText3);
                    acTrans.AddNewlyCreatedDBObject(acText3, true);
                }
                #endregion
                acTrans.Commit();
            }
        }
        [CommandMethod("temelalti")]
        public void temelalti()
        {
            kesismenoktalari = new List<Point3d>();
            Xaksnoktalaribaslangic = new List<Point3d>();
            Xaksnoktalaribitis = new List<Point3d>();
            Yaksnoktalaribaslangic = new List<Point3d>();
            Yaksnoktalaribitis = new List<Point3d>();
            Point3d centerpoint;
            Document doc = Application.DocumentManager.MdiActiveDocument;
            Database db = doc.Database;
            Editor ed = doc.Editor;
            PromptEntityOptions peo = new PromptEntityOptions("\nSelect a polyline: ");
            peo.SetRejectMessage("Only a polyline !");
            peo.AddAllowedClass(typeof(Autodesk.AutoCAD.DatabaseServices.Polyline), true);
            PromptEntityResult per = ed.GetEntity(peo);
            if (per.Status != PromptStatus.OK)
                return;
            else
            {
                ObjectId ObjID = per.ObjectId;
                centerpoint = Metods.MerkezNokta(ObjID);
            }
            double[] kriterler = Metods.KriterleriBelirle();
            using (Transaction tr = db.TransactionManager.StartOpenCloseTransaction())
            {
                double yatayaksaraligi = kriterler[0];
                double dikeyaksaraligi = kriterler[1];
                cembercap = kriterler[2];
                double soldangirintimiktari = cembercap / 2;
                double yukaridangirintimiktari = cembercap / 2;


                Autodesk.AutoCAD.DatabaseServices.Polyline plineread = (Autodesk.AutoCAD.DatabaseServices.Polyline)tr.GetObject(per.ObjectId, OpenMode.ForRead);
                Autodesk.AutoCAD.DatabaseServices.Polyline plinewrite = (Autodesk.AutoCAD.DatabaseServices.Polyline)tr.GetObject(per.ObjectId, OpenMode.ForWrite);
                plinewrite.Closed = true;
                plineread = plinewrite;
                double xmax = plineread.Bounds.Value.MaxPoint.X;
                double ymin = plineread.Bounds.Value.MinPoint.Y;
                double ymax = plineread.Bounds.Value.MaxPoint.Y;
                double xmin = plineread.Bounds.Value.MinPoint.X;

                Point3d startpoint = new Point3d(plineread.StartPoint.X + soldangirintimiktari, plineread.StartPoint.Y - yukaridangirintimiktari, plineread.StartPoint.Z);

                AksCemberleri(startpoint, yatayaksaraligi, dikeyaksaraligi, plineread, cembercap / 2);
                YatayAks(yatayaksaraligi, startpoint, xmax);
                DikeyAks(dikeyaksaraligi, startpoint, ymin);
                startpoint = new Point3d(startpoint.X + soldangirintimiktari, startpoint.Y - yukaridangirintimiktari, startpoint.Z);
                if (solagidilmedurumu)//eğer sola gidilme durumu varsa kesişen noktalar yani çember merkezlerinin sıralaması yeniden yapılacak.
                {
                    kesismenoktalari = kesismenoktalari.OrderByDescending(p => p.Y).ThenBy(p => p.X).ToList();
                    YaziOlustur(NoktalariGüncelle(kesismenoktalari, 30, 30), "K");
                }
                else
                    YaziOlustur(NoktalariGüncelle(kesismenoktalari, 30, 30), "K");

                BitisCemberleriX_ust(NoktalariGüncelle2(Xaksnoktalaribaslangic, 0, 30), "X", "xust");
                BitisCemberleriX_ust(NoktalariGüncelle(Xaksnoktalaribitis, 0, 30), "X", "xalt");
                BitisCemberleriX_ust(NoktalariGüncelle2(Yaksnoktalaribaslangic, 30, 0), "Y", "ysol");
                BitisCemberleriX_ust(NoktalariGüncelle(Yaksnoktalaribitis, 30, 0), "Y", "ysag");

                XaksOlcuOlustur(Xaksnoktalaribaslangic, dikeyaksaraligi);
                XaksOlcuOlustur(Xaksnoktalaribitis, dikeyaksaraligi);
                YaksOlcuOlustur(Yaksnoktalaribaslangic, yatayaksaraligi);
                YaksOlcuOlustur(Yaksnoktalaribitis, yatayaksaraligi);
                articiz(centerpoint, ymin, ymax, xmin, xmax);
                GirintileriYazdir(cembercap/2, plineread);
            }
        }




    }

}
