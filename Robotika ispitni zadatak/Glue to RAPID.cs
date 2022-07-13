using Grasshopper.Kernel;
using Grasshopper.Kernel.Data;
using Grasshopper.Kernel.Types;
using Rhino.Geometry;
using System;
using System.Collections.Generic;

namespace Robotika_ispitni_zadatak
{
    public class GlueToRAPID : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the GlueToRAPID class.
        /// </summary>
        public GlueToRAPID()
          : base("Glue to RAPID", "GlueRAPID",
              "Pozicije na kvadrovima na kojima se nanosi lepak",
              "ToRAPID", "Gredice")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddPointParameter("Origin Glue", "OG", "Centralna tačka za nanošenje lepka", GH_ParamAccess.tree);
            pManager.AddPointParameter("Origin Robot", "OR", "Centar pozicije robota", GH_ParamAccess.item);
            pManager.AddPointParameter("wobj", "WO", "Work object za poziciju tacaka lepka", GH_ParamAccess.item);
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
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            GH_Structure<GH_Point> tackeOG = new GH_Structure<GH_Point>();
            Point3d tackaOR = new Point3d();

            string RAPID;

            int brojGrana;
            List<int> brojElemenata = new List<int>();
            Point3d tempOR;
            Point3d tempOG;
            double tempOGX;
            double tempOGY;
            double tempOGZ;
            Vector3d tempVX = new Vector3d();
            Vector3d tempVY = new Vector3d();
            Vector3d tempVZ = new Vector3d(0, 0, -1);
            int brojac = 0;
            int maxEl = 0;
            string bonusTarget = "Target_Home";
            Point3d wobj_lepak = new Point3d();

            DA.GetDataTree(0, out tackeOG);
            DA.GetData(1,ref tackaOR);
            DA.GetData(2, ref wobj_lepak);

            brojGrana = tackeOG.Branches.Count;

            //Popisivanje broja elemenata za svaku granu
            //i određivanje dužine najduže grane zato
            //što RAPID ne podržava nestovanje listi
            //različitih dužina
            for (int i = 0; i < brojGrana; i++)
            {

                brojElemenata.Add(tackeOG[i].Count);

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
                    tempOG = tackeOG[b][l].Value;
                    tempOGX = tackeOG[b][l].Value.X - wobj_lepak.X;
                    tempOGY = tackeOG[b][l].Value.Y - wobj_lepak.Y;
                    tempOGZ = tackeOG[b][l].Value.Z - wobj_lepak.Z;

                    tempOR = tackaOR;
                    tempOR.Z = tempOGZ;

                    tempVY.X = tempOR.X - tempOGX;
                    tempVY.Y = tempOR.Y - tempOGY;
                    tempVY.Z = tempOR.Z - tempOGZ;

                    tempVY.Unitize();

                    tempVX = Vector3d.CrossProduct(tempVY, tempVZ);

                    Quaternion q = new Quaternion();
                    Plane p1 = new Plane(tempOG, tempVX, tempVY);
                    Plane p2 = new Plane(new Point3d(0, 0, 0), new Vector3d(1, 0, 0), new Vector3d(0, 1, 0));
                    q = Quaternion.Rotation(p2, p1);

                    RAPID += "\r\n CONST robtarget Target_" + brojac.ToString() + ":=[[" + tempOGX.ToString() + "," + tempOGY.ToString() + "," + tempOGZ.ToString() + "],[" + q.A.ToString() + ", " + q.B.ToString() + ", " + q.C.ToString() + ", " + q.D.ToString() + "],[0,0,-1,0],[9E+09,9E+09,9E+09,9E+09,9E+09,9E+09]];";

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
        /// Provides an Icon for the component.
        /// </summary>
        protected override System.Drawing.Bitmap Icon
        {
            get
            {
                //You can add image files to your project resources and access them like this:
                // return Resources.IconForThisComponent;
                return Properties.Resources.GlueRAPID;
            }
        }

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("d56df87c-924b-4a8f-8184-505387c8879f"); }
        }
    }
}