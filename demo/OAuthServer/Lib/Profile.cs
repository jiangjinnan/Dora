using System;

namespace Lib
{
    public class Profile
    {
        //Acount related
        public string UserName { get; set; }
        public string Email { get; set; }

        //Personal
        public string Surname { get; set; }
        public string GivenName { get; set; }
        public string Gender { get; set; }
        public string BirthDate { get; set; }
    }
}
