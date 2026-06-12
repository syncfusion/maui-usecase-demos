using System;
using System.Collections.Generic;
using System.Text;
using System.Collections.ObjectModel;
using System.Globalization;

namespace ContactsApp
{
    public class ContactsViewModel
    {
        public ObservableCollection<Contact> Contacts { get; set; }

        public ObservableCollection<string> Alphabet { get; set; }

        public ContactsViewModel()
        {
            Contacts = new ObservableCollection<Contact>(LoadContacts());

            Alphabet = new ObservableCollection<string>(
                Enumerable.Range('A', 26).Select(c => ((char)c).ToString()));
        }

        private List<Contact> LoadContacts()
        {
            return new List<Contact>
    {
        // A
        new Contact { Name = "Aarav", PhoneNumber="80000001" },
        new Contact { Name = "Aisha", PhoneNumber="80000002" },
        new Contact { Name = "Amit", PhoneNumber="80000003" },
        new Contact { Name = "Anaya", PhoneNumber="80000004" },
        new Contact { Name = "Arjun", PhoneNumber="80000005" },
        new Contact { Name = "Ayra", PhoneNumber="80000006" },

        // B
        new Contact { Name = "Bala", PhoneNumber="80000007" },
        new Contact { Name = "Bhuvi", PhoneNumber="80000008" },
        new Contact { Name = "Bhavesh", PhoneNumber="80000009" },
        new Contact { Name = "Bhavya", PhoneNumber="80000010" },
        new Contact { Name = "Bharat", PhoneNumber="80000011" },

        // C
        new Contact { Name = "Charan", PhoneNumber="80000012" },
        new Contact { Name = "Chitra", PhoneNumber="80000013" },
        new Contact { Name = "Chloe", PhoneNumber="80000014" },
        new Contact { Name = "Carter", PhoneNumber="80000015" },
        new Contact { Name = "Cyril", PhoneNumber="80000016" },

        // D
        new Contact { Name = "Deepak", PhoneNumber="80000017" },
        new Contact { Name = "Divya", PhoneNumber="80000018" },
        new Contact { Name = "Dinesh", PhoneNumber="80000019" },
        new Contact { Name = "Dhruv", PhoneNumber="80000020" },
        new Contact { Name = "Diya", PhoneNumber="80000021" },

        // E
        new Contact { Name = "Esha", PhoneNumber="80000022" },
        new Contact { Name = "Ethan", PhoneNumber="80000023" },
        new Contact { Name = "Elias", PhoneNumber="80000024" },
        new Contact { Name = "Eva", PhoneNumber="80000025" },
        new Contact { Name = "Emil", PhoneNumber="80000026" },

        // F
        new Contact { Name = "Farhan", PhoneNumber="80000027" },
        new Contact { Name = "Fathima", PhoneNumber="80000028" },
        new Contact { Name = "Fiona", PhoneNumber="80000029" },
        new Contact { Name = "Faiz", PhoneNumber="80000030" },
        new Contact { Name = "Faris", PhoneNumber="80000031" },

        // G
        new Contact { Name = "Gokul", PhoneNumber="80000032" },
        new Contact { Name = "Gauri", PhoneNumber="80000033" },
        new Contact { Name = "Girish", PhoneNumber="80000034" },
        new Contact { Name = "Gowri", PhoneNumber="80000035" },
        new Contact { Name = "Gagan", PhoneNumber="80000036" },

        // H
        new Contact { Name = "Hari", PhoneNumber="80000037" },
        new Contact { Name = "Harini", PhoneNumber="80000038" },
        new Contact { Name = "Hemanth", PhoneNumber="80000039" },
        new Contact { Name = "Hina", PhoneNumber="80000040" },
        new Contact { Name = "Hassan", PhoneNumber="80000041" },

        // I
        new Contact { Name = "Irfan", PhoneNumber="80000042" },
        new Contact { Name = "Isha", PhoneNumber="80000043" },
        new Contact { Name = "Imran", PhoneNumber="80000044" },
        new Contact { Name = "Ira", PhoneNumber="80000045" },
        new Contact { Name = "Ibrahim", PhoneNumber="80000046" },

        // J
        new Contact { Name = "Jai", PhoneNumber="80000047" },
        new Contact { Name = "Jaya", PhoneNumber="80000048" },
        new Contact { Name = "Johan", PhoneNumber="80000049" },
        new Contact { Name = "Jasmine", PhoneNumber="80000050" },
        new Contact { Name = "Junaid", PhoneNumber="80000051" },

        // K
        new Contact { Name = "Karthik", PhoneNumber="80000052" },
        new Contact { Name = "Kaviya", PhoneNumber="80000053" },
        new Contact { Name = "Kiran", PhoneNumber="80000054" },
        new Contact { Name = "Kavya", PhoneNumber="80000055" },
        new Contact { Name = "Kamal", PhoneNumber="80000056" },

        // L
        new Contact { Name = "Lakshmi", PhoneNumber="80000057" },
        new Contact { Name = "Latha", PhoneNumber="80000058" },
        new Contact { Name = "Leo", PhoneNumber="80000059" },
        new Contact { Name = "Leena", PhoneNumber="80000060" },
        new Contact { Name = "Lokesh", PhoneNumber="80000061" },

        // M
        new Contact { Name = "Manoj", PhoneNumber="80000062" },
        new Contact { Name = "Meena", PhoneNumber="80000063" },
        new Contact { Name = "Mohan", PhoneNumber="80000064" },
        new Contact { Name = "Mira", PhoneNumber="80000065" },
        new Contact { Name = "Muthu", PhoneNumber="80000066" },

        // N
        new Contact { Name = "Naveen", PhoneNumber="80000067" },
        new Contact { Name = "Nithya", PhoneNumber="80000068" },
        new Contact { Name = "Nikhil", PhoneNumber="80000069" },
        new Contact { Name = "Nisha", PhoneNumber="80000070" },
        new Contact { Name = "Nasir", PhoneNumber="80000071" },

        // O
        new Contact { Name = "Omkar", PhoneNumber="80000072" },
        new Contact { Name = "Oviya", PhoneNumber="80000073" },
        new Contact { Name = "Omar", PhoneNumber="80000074" },
        new Contact { Name = "Olivia", PhoneNumber="80000075" },
        new Contact { Name = "Oscar", PhoneNumber="80000076" },

        // P
        new Contact { Name = "Prakash", PhoneNumber="80000077" },
        new Contact { Name = "Priya", PhoneNumber="80000078" },
        new Contact { Name = "Pranav", PhoneNumber="80000079" },
        new Contact { Name = "Pooja", PhoneNumber="80000080" },
        new Contact { Name = "Parth", PhoneNumber="80000081" },

        // Q
        new Contact { Name = "Qasim", PhoneNumber="80000082" },
        new Contact { Name = "Qadir", PhoneNumber="80000083" },
        new Contact { Name = "Qiana", PhoneNumber="80000084" },
        new Contact { Name = "Qureshi", PhoneNumber="80000085" },

        // R
        new Contact { Name = "Ravi", PhoneNumber="80000086" },
        new Contact { Name = "Rekha", PhoneNumber="80000087" },
        new Contact { Name = "Rohan", PhoneNumber="80000088" },
        new Contact { Name = "Riya", PhoneNumber="80000089" },
        new Contact { Name = "Rahul", PhoneNumber="80000090" },

        // S
        new Contact { Name = "Suresh", PhoneNumber="80000091" },
        new Contact { Name = "Sneha", PhoneNumber="80000092" },
        new Contact { Name = "Sanjay", PhoneNumber="80000093" },
        new Contact { Name = "Swathi", PhoneNumber="80000094" },
        new Contact { Name = "Siddharth", PhoneNumber="80000095" },

        // T
        new Contact { Name = "Tamil", PhoneNumber="80000096" },
        new Contact { Name = "Tanu", PhoneNumber="80000097" },
        new Contact { Name = "Tarun", PhoneNumber="80000098" },
        new Contact { Name = "Teja", PhoneNumber="80000099" },
        new Contact { Name = "Tina", PhoneNumber="80000100" },

        // U
        new Contact { Name = "Uday", PhoneNumber="80000101" },
        new Contact { Name = "Uma", PhoneNumber="80000102" },
        new Contact { Name = "Umar", PhoneNumber="80000103" },
        new Contact { Name = "Usha", PhoneNumber="80000104" },
        new Contact { Name = "Utkarsh", PhoneNumber="80000105" },

        // V
        new Contact { Name = "Vijay", PhoneNumber="80000106" },
        new Contact { Name = "Vidya", PhoneNumber="80000107" },
        new Contact { Name = "Varun", PhoneNumber="80000108" },
        new Contact { Name = "Vani", PhoneNumber="80000109" },
        new Contact { Name = "Vikram", PhoneNumber="80000110" },

        // W
        new Contact { Name = "Wasim", PhoneNumber="80000111" },
        new Contact { Name = "Waseem", PhoneNumber="80000112" },
        new Contact { Name = "William", PhoneNumber="80000113" },
        new Contact { Name = "Wafa", PhoneNumber="80000114" },

        // X
        new Contact { Name = "Xavier", PhoneNumber="80000115" },
        new Contact { Name = "Xena", PhoneNumber="80000116" },
        new Contact { Name = "Xander", PhoneNumber="80000117" },

        // Y
        new Contact { Name = "Yusuf", PhoneNumber="80000118" },
        new Contact { Name = "Yamini", PhoneNumber="80000119" },
        new Contact { Name = "Yash", PhoneNumber="80000120" },
        new Contact { Name = "Yara", PhoneNumber="80000121" },

        // Z
        new Contact { Name = "Zain", PhoneNumber="80000122" },
        new Contact { Name = "Zara", PhoneNumber="80000123" },
        new Contact { Name = "Zubair", PhoneNumber="80000124" },
        new Contact { Name = "Zoya", PhoneNumber="80000125" }
    };
        }
    }

}
