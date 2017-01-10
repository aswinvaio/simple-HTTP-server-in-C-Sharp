using Xhtml;

namespace XhtmlFields
{
    public class XhtmlFields : XhtmlPage
    {
        private string _Var1;

        public string Var1
        {
            get { return _Var1; }
            set { _Var1 = value; }
        }

        private string _Var2 = "Var2 is empty";

        public string Var2
        {
            get { return _Var2; }
            set { _Var2 = value; }
        }

        private string _Var3 = "808080";

        public string Var3
        {
            get { return _Var3; }
            set { _Var3 = value; }
        }

    }
}
