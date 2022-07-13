using Grasshopper.Kernel;
using Grasshopper.Kernel.Data;
using Grasshopper.Kernel.Types;
using Rhino.Geometry;
using System;
using System.Collections.Generic;

// In order to load the result of this wizard, you will also need to
// add the output bin/ folder of this project to the list of loaded
// folder in Grasshopper.
// You can use the _GrasshopperDeveloperSettings Rhino command for that.

namespace Robotika_ispitni_zadatak
{
    public class BoxPositionToRAPID : GH_Component
    {
        /// <summary>
        /// Each implementation of GH_Component must provide a public 
        /// constructor without any arguments.
        /// Category represents the Tab in which the component will appear, 
        /// Subcategory the panel. If you use non-existing tab or panel names, 
        /// new tabs/panels will automatically be created.
        /// </summary>
        public BoxPositionToRAPID()
          : base("Box position to RAPID", "BoxRAPID",
              "Pretvaranje koordinata na kvadru u targete za ABB robote",
              "ToRAPID", "Gredice")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddPointParameter("Origin", "O", "Tacke na gredicama za pozicioniranje vrha sisaljke", GH_ParamAccess.tree);
            pManager.AddVectorParameter("Vektor X", "X","Vektor upravan na podužnu osu gredice", GH_ParamAccess.tree);
            pManager.AddVectorParameter("Vektor Y", "Y", "Vektor u podužnoj osi gredice", GH_ParamAccess.tree);
            pManager.AddPointParameter("wobj", "WO", "Pozicija work objecta za gredice", GH_ParamAccess.item);
            
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddTextParameter("RAPID", "R", "Rapid studio kod", GH_ParamAccess.item);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object can be used to retrieve data from input parameters and 
        /// to store data in output parameters.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            //Promenljive koje prihvataju podatke sa ulaza
            //struktuirana kao nestovane liste podeljene po
            //nivou grede
            GH_Structure<GH_Point> tackeO = new GH_Structure<GH_Point>();
            GH_Structure<GH_Vector> vektorX = new GH_Structure<GH_Vector>();
            GH_Structure<GH_Vector> vektorY = new GH_Structure<GH_Vector>();
            //string u koji ide kod
            string RAPID;
            //pomoćne promenljive za povlačenje informacija iz ulaznih promenljivih
            int brojGrana;
            List<int> brojElemenata = new List<int>();
            Point3d tempO;
            double tempOX;
            double tempOY;
            double tempOZ;
            Vector3d tempVX;
            Vector3d tempVY;
            int brojac = 0;
            int maxEl = 0;
            string bonusTarget = "Target_Home";
            Point3d wobj_gredice = new Point3d();

            DA.GetDataTree(0, out tackeO);
            DA.GetDataTree(1, out vektorX);
            DA.GetDataTree(2, out vektorY);
            DA.GetData(3, ref wobj_gredice);

            brojGrana = tackeO.Branches.Count;
            
            //Popisivanje broja elemenata za svaku granu
            //i određivanje dužine najduže grane zato
            //što RAPID ne podržava nestovanje listi
            //različitih dužina
            for (int i = 0; i < brojGrana; i++)
            {
                
                brojElemenata.Add(tackeO[i].Count);
                
                if (brojElemenata[i] > maxEl)
                {
                    maxEl = brojElemenata[i];
                }
            }
            
            //Pretvaranje ulaznih podataka u
            //spisak targeta
            RAPID = "MODULE Targets\r\n";
            
            for (int b = 0; b < brojGrana; b++)
            {
                for (int l = 0; l < brojElemenata[b]; l++)
                {
                    tempO = tackeO[b][l].Value;
                    tempOX = tackeO[b][l].Value.X-wobj_gredice.X;
                    tempOY = tackeO[b][l].Value.Y-wobj_gredice.Y;
                    tempOZ = tackeO[b][l].Value.Z-wobj_gredice.Z;
                    tempVX = vektorX[b][l].Value;
                    tempVY = vektorY[b][l].Value;

                    Quaternion q = new Quaternion();
                    Plane p1 = new Plane(tempO, tempVX, tempVY);
                    Plane p2 = new Plane(new Point3d(0, 0, 0), new Vector3d(1, 0, 0), new Vector3d(0, 1, 0));
                    q = Quaternion.Rotation(p2, p1);

                    RAPID += "\r\n CONST robtarget Target_" + brojac.ToString() + ":=[[" + tempOX.ToString() + "," + tempOY.ToString() + "," + tempOZ.ToString() + "],[" + q.A.ToString() + ", " + q.B.ToString() + ", " + q.C.ToString() + ", " + q.D.ToString() + "],[0,0,-1,0],[9E+09,9E+09,9E+09,9E+09,9E+09,9E+09]];";

                    brojac++;
                }
            }

            brojac = 0;

            //Pravljenje nestovane liste
            //Viškovi pozicija se popunjavaju sa Target_Home koji
            //se neće pozivati (u suštini može biti bilo koji target)
            RAPID += "\r\n \r\n VAR robtarget Target_Nivoi_List {" + brojGrana + "," + maxEl + "}:=[";

            for (int b = 0; b < brojGrana; b++)
            {

                if (b == 0)
                    RAPID += "[";
                else
                    RAPID += ",[";

                for (int l = 0; l < brojElemenata[b]; l++)
                {
                    if (l == 0)
                        RAPID += "Target_" + brojac;
                    else
                        RAPID += ",Target_" + brojac;

                    brojac++;
                }
                if (maxEl - brojElemenata[b] > 0)
                {
                    for (int c = 0; c < maxEl - brojElemenata[b]; c++)
                    {
                        RAPID += "," + bonusTarget;
                    }
                }

                RAPID += "]";
            }

            RAPID += "]; \r\n";

            //Kreiranje promenljivih koji će olakšati
            //pravljenje for petlji u Main proc u Robot Studiu
            RAPID += "\r\n CONST num brNivoa :=" + brojGrana + ";";
            RAPID += "\r\n VAR num brGredica{" + brojGrana + "}:=[";
            for (int i = 0; i < brojElemenata.Count; i++)
            {
                if (i == 0)
                    RAPID += brojElemenata[i];
                else
                    RAPID += "," + brojElemenata[i];
            }
            RAPID += "];\r\n";
            RAPID += "\r\nENDMODULE";
            
            DA.SetData(0, RAPID);

        }

        /// <summary>
        /// Provides an Icon for every component that will be visible in the User Interface.
        /// Icons need to be 24x24 pixels.
        /// </summary>
        protected override System.Drawing.Bitmap Icon
        {
            get
            {
                // You can add image files to your project resources and access them like this:
                //return Resources.IconForThisComponent;
                return Properties.Resources.BoxTargRAPID;
            }
        }

        /// <summary>
        /// Each component must have a unique Guid to identify it. 
        /// It is vital this Guid doesn't change otherwise old ghx files 
        /// that use the old ID will partially fail during loading.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("74885f2e-2d52-4c60-b7df-396719ad32ab"); }
        }
    }
}
