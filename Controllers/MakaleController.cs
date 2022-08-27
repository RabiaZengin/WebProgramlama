using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using WebProgramlamaProje.Helpers;
using WebProgramlamaProje.Models;
using WebProgramlamaProje.ViewModels;

namespace WebProgramlamaProje.Controllers
{
    public class MakaleController : YetkiliController
    {
        BlogDB db = new BlogDB();

        // GET: Makale
        public ActionResult Index(string AramaYap=null,int KategoriId=0)  //eğer bir değer gelmezse değeri null yap
        {
            ViewBag.KategoriId = new SelectList(db.Kategoris, "KategoriId", "KategoriAdi");

            var makaleler = from a in db.Makales
                            select a;
                           
            if (KategoriId != 0)
            {
                makaleler = makaleler.Where(i => i.KategoriId == KategoriId);
            }
            if (!string.IsNullOrEmpty(AramaYap))
            {
                makaleler = makaleler.Where(i => i.Baslık.Contains(AramaYap));
            }
            return View(makaleler);
        }

        // GET: Makale/Details/5
        public ActionResult Details(int id)
        {
            var makale = db.Makales.Where(i => i.Id == id).SingleOrDefault();

            if(makale==null)
            {
                return HttpNotFound();
            }

            SonAtilanMakaleViewModel vm = new SonAtilanMakaleViewModel();
            vm.Makalem = makale;
            vm.SonMakaleler = db.Makales.OrderByDescending(i => i.Tarih).Take(5).ToList();
            return View(vm);
        }
        public ActionResult KisiMakaleListele()
        {
            var kullanıcıadi = Session["username"].ToString();
            var makaleler = db.Kullanicis.Where(a => a.KullaniciAdi == kullanıcıadi).SingleOrDefault().Makales.ToList();
            return View(makaleler);
        }

        // GET: Makale/Create
        public ActionResult Create()  //View ekledim
        {
            ViewBag.KategoriId = new SelectList(db.Kategoris, "KategoriId", "KategoriAdi");
            return View();
        }

        // POST: Makale/Create
        [HttpPost]
        public ActionResult Create(Makale model, string etiketler)
        {
            try
            {
                // TODO: Add insert logic here
                string kullaniciadi = Session["username"].ToString();
                var kullanici = db.Kullanicis.Where(i => i.KullaniciAdi == kullaniciadi).SingleOrDefault();
                model.KullaniciId = kullanici.Id;
                model.Tarih = DateTime.Now;
                db.Makales.Add(model);


                if (!string.IsNullOrEmpty(etiketler))
                {
                    string[] etiketDizisi = etiketler.Split(',');
                    foreach (var i in etiketDizisi)
                    {
                        var yeniEtiket = new Etiket { EtiketAd = i };
                        db.Etikets.Add(yeniEtiket);
                        model.Etikets.Add(yeniEtiket);
                    }
                }
                db.SaveChanges();
               
                return RedirectToAction("Index","Kullanici");
            }
            catch
            {
                return View();
            }
        }

        // GET: Makale/Edit/5
        public ActionResult Edit(int id)
        {
            string kullaniciadi = Session["username"].ToString();
            var makale = db.Makales.Where(i => i.Id == id).SingleOrDefault();
            if (makale == null)
            {
                return HttpNotFound();

            }
            if (makale.Kullanici.KullaniciAdi==kullaniciadi)
            {
                ViewBag.KategoriId = new SelectList(db.Kategoris, "KategoriId", "KategoriAdi");

                return View(makale);
            }
            return HttpNotFound();
        }

        // POST: Makale/Edit/5
        [HttpPost]
        public ActionResult Edit(int id,  Makale model)
        {
            try
            {
                // TODO: Add update logic here
                var makale = db.Makales.Where(i => i.Id == id).SingleOrDefault();
                makale.Baslık = model.Baslık;
                makale.Icerik = model.Icerik;
                makale.KategoriId = model.KategoriId;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            catch
            {
                return View(model);
            }
        }

        public ActionResult Delete(int id)
        {
            try
            {

                var kullaniciadi = Session["username"].ToString();
                var kullanici = db.Kullanicis.Where(i => i.KullaniciAdi == kullaniciadi).SingleOrDefault();
                var makale = db.Makales.Where(i => i.Id == id).SingleOrDefault();
                if(kullanici.Id==makale.KullaniciId)
                {
                    foreach (var i in makale.Yorums.ToList())
                    {
                        db.Yorums.Remove(i);
                    }
                    foreach(var i in makale.Etikets.ToList())
                    {
                        db.Etikets.Remove(i);

                    }
                    db.Makales.Remove(makale);
                    db.SaveChanges();
                    return RedirectToAction("Index");

                }
                return RedirectToAction("Hata", "Yetkili", new { yazilacak = "Makale Silinemedi!" });
            }
            catch
            {
                return RedirectToAction("Hata", "Yetkili", new { yazilacak = "Makale Silinemedi!" });
            }
        }


        public JsonResult YorumYap(string yorum, int Makaleid)
        {
            var kullaniciadi = Session["username"].ToString();
            var kullanici = db.Kullanicis.Where(i => i.KullaniciAdi == kullaniciadi).SingleOrDefault();

            if(yorum==null)
            {
                return Json(true, JsonRequestBehavior.AllowGet);


            }

            db.Yorums.Add(new Yorum { KullanıcıId = kullanici.Id, MakaleId = Makaleid, Tarih = DateTime.Now, YorumIcerik = yorum });
            db.SaveChanges();
            return Json(false,JsonRequestBehavior.AllowGet);
        }
        public ActionResult YorumDelete(int id)
        {
            try
            {
                var kullaniciadi = Session["username"].ToString();
                var kullanici = db.Kullanicis.Where(i => i.KullaniciAdi == kullaniciadi).SingleOrDefault();
                var yorum = db.Yorums.Where(i => i.Id == id).SingleOrDefault();
                var makale = db.Makales.Where(i => i.Id == yorum.MakaleId).SingleOrDefault();
                if (yorum == null)
                {
                    return RedirectToAction("Hata", "Yetkili", new { yazilacak = "Yorum Bulunamadı!" });
                }
                 if (OrtakSinif.DeleteIzinYetkiVarmi(id, kullanici) || makale.KullaniciId == kullanici.Id)
                {
                    db.Yorums.Remove(yorum);
                    db.SaveChanges();
                    return RedirectToAction("Details", "Makale", new { id = yorum.MakaleId });

                }
                return RedirectToAction("Hata", "Yetkili", new { yazilacak = "Yorum Silinemedi! Yetkiniz Yok!" });

            }
            catch 
            {
                return RedirectToAction("Hata", "Yetkili", new { yazilacak = "Yorum Silinemedi!" });


            }

        }

    }
}
