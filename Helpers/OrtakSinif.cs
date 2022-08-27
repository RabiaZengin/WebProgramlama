using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using WebProgramlamaProje.Models;

namespace WebProgramlamaProje.Helpers
{
    public class OrtakSinif
    {
        BlogDB db = new BlogDB();

        public static bool EditIzinYetkiVarmi(int id,Kullanici user)//Kullanıcı bilgilerinde admin ve süper admin yetkisine sahip kullanıcın erişimine izin veriyoruz
        {

            
            if (user.Id == id)
            {
                return true;

            }
            else if (user.YetkiId > 2)
            {
                return true;

            }
            return false;


        }

        public static bool DeleteIzinYetkiVarmi(int id, Kullanici user) //yetki ıd sisüper amin olan kullanıcının erişimine izin veriyoruz.
        {

            //int id olarak tanımlanan kısım kullanıcıId dir.
            if (user.Id == id)
            {
                return true;

            }
            else if (user.YetkiId > 3)
            {
                return true;

            }
            return false;


        }



    }
}