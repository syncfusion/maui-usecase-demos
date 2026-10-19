using System;
using System.Collections.Generic;
using System.Text;

namespace ContactsApp
{
    public class Contact
    {
        public string Name { get; set; }

        public string PhoneNumber { get; set; }

        public string Initial => string.IsNullOrEmpty(Name)
            ? "?"
            : Name.Substring(0, 1).ToUpper();

        public string GroupKey => Initial;
    }

}
