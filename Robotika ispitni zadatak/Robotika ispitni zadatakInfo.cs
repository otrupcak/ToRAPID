using Grasshopper.Kernel;
using Grasshopper;
using System;
using System.Drawing;

namespace Robotika_ispitni_zadatak
{
    public class Robotika_ispitni_zadatakInfo : GH_AssemblyInfo
    {
        public override string Name
        {
            get
            {
                return "ToRAPID";
            }
        }
        public override Bitmap Icon
        {
            get
            {
                //Return a 24x24 pixel bitmap to represent this GHA library.
                return null;
            }
        }
        public override string Description
        {
            get
            {
                //Return a short string describing the purpose of this GHA library.
                return "Alati za finalni projekat iz robotike generacija A7/2020";
            }
        }
        public override Guid Id
        {
            get
            {
                return new Guid("a659e1e2-c786-4e93-9bc4-d221f035f911");
            }
        }

        public override string AuthorName
        {
            get
            {
                //Return a string identifying you or your company.
                return "Zoran";
            }
        }
        public override string AuthorContact
        {
            get
            {
                //Return a string representing your preferred contact details.
                return "otrupcak@gmail.com";
            }
        }



        }
    }
