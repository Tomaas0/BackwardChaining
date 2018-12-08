using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace BackwardChaining
{
    public static class BackwardChainingJob
    {
        #region Kintamieji

        static Change lastChange;
        static Boolean sekme;
        static int gylis;
        static List<Char> Tikslai = new List<Char>();
        static GDB db;

        #endregion
        private static void Gryzti(Boolean Success = false)
        {
            Change change = db.Changes.Pop();
            Tikslai = change.SeniTikslai;
            if (!Success) db.Faktai = change.SeniFaktai;
            if (!Success) db.Kelias = change.SeniKeliai;
            //if(!Success) change.PanaudotaProjekcija.Flag = change.ProjekcijosSenasFlag;

            /*if (!Success)
            {
                while(db.Changes.Peek().Gylis >= gylis)
                {
                    db.Changes.Pop();
                }
            }*/
            lastChange = change;
        }
        public static void Run(GDB Db)
        {
            db = Db;
            #region Protokolas 1 dalis
            string output;
            StreamWriter file = new StreamWriter(String.Format("{0} protokolas.txt", db.TestName), false);
            file.WriteLine("1 DALIS. Duomenys");
            file.WriteLine("");
            file.WriteLine("  1) Taisyklės");
            foreach (Projekcija p in db.Projekcijos)
            {
                file.WriteLine(String.Format("    {0}", p));
            }
            file.WriteLine("");
            file.WriteLine("  2) Faktai");
            output = String.Format("    {0}", db.InitFaktai.ElementAt(0));
            for (int i = 1; i < db.InitFaktai.Count; i++)
            {
                output += String.Format(", {0}", db.InitFaktai.ElementAt(i));
            }
            file.WriteLine(output);
            file.WriteLine("");
            file.WriteLine("  2) Tikslas");
            file.WriteLine(String.Format("    {0}", db.Tikslas));
            file.WriteLine("");
            #endregion

            #region Vykdymas
            if (db.InitFaktai.Contains(db.Tikslas))
            {
                file.WriteLine("3 DALIS. Rezultatai");
                file.WriteLine(String.Format("  Tikslas {0} tarp faktų. Kelias tuščias.", db.Tikslas));
                file.Close();
                return;
            }
            file.WriteLine("2 DALIS. Vykdymas");
            file.WriteLine("");
            int iCount = 0; //Iteraciju skaitliukas
            gylis = 0;
            file.AutoFlush = true;//NUIMTI SITAAAAAAAA
            bool done = false;
            Tikslai.Add(db.Tikslas);
            sekme = false;
            while (!done)
            {
                iCount++;
                String line = "";
                Char Tikslas = Tikslai.ElementAt(0);
                line += String.Format("  {0}) ", iCount.ToString());
                for(int i = 0; i < gylis; i++)
                {
                    line += "-";
                }
                line += String.Format("Tikslas {0}. ", Tikslas);
                

                if (sekme)
                {
                    var a = lastChange.NaujiTikslai.Intersect(lastChange.SeniTikslai).ToList();
                    var tikslasToCheck = Tikslai.Count > 1 ? Tikslai.ElementAt(1) : '`';
                    if (lastChange.NaujiTikslai.Intersect(lastChange.SeniTikslai).ToList().Contains(tikslasToCheck))
                    {
                        db.Faktai.Add(Tikslas);
                        Tikslai.RemoveAt(0);
                        line += String.Format("Faktas(dabar gautas). Faktai {0}.", db.FaktaiToString);
                        line += " Grįžtame, sėkmė.";

                        sekme = false;
                        
                        for(int i = 0; i < db.Projekcijos.Count; i++)
                        {
                            db.Projekcijos.ElementAt(i).Flag = db.Changes.Peek().ProjekcijuNaujiFlag.ElementAt(i);
                            //db.Projekcijos.ElementAt(i).Flag = lastChange.ProjekcijuSeniFlag.ElementAt(i);
                        }
                    }
                    else
                    {
                        db.Faktai.Add(Tikslas);
                        line += String.Format("Faktas(dabar gautas). Faktai {0}.", db.FaktaiToString);
                        if (db.Faktai.Contains(db.Tikslas))
                        {
                            line += " Sėkmė.";
                            done = true;
                        }
                        else
                        {
                            line += " Grįžtame, sėkmė.";
                            gylis--;

                            Gryzti(true);
                            sekme = true;
                            db.Kelias.Add(lastChange.PanaudotaProjekcija);
                        }
                    }
                }
                else if (db.Changes.Select(a => a.IeskotasTikslas).Contains(Tikslas))
                {
                    line += String.Format("Ciklas. Grįžtame, FAIL.");
                    gylis--;

                    Gryzti();
                    sekme = false;
                }
                else if (db.InitFaktai.Contains(Tikslas))
                {
                    line += String.Format("Faktas(duotas), nes faktai {0}.", db.FaktaiToString);
                    line += " Grįžtame, sėkmė.";
                    gylis--;

                    Gryzti(true);
                    sekme = true;

                    db.Kelias.Add(lastChange.PanaudotaProjekcija);
                }
                else if (db.Faktai.Contains(Tikslas))
                {
                    line += String.Format("Faktas(buvo gautas). Faktai {0}.", db.FaktaiToString);
                    line += " Grįžtame, sėkmė.";
                    gylis--;

                    Gryzti(true);
                    sekme = true;

                    db.Kelias.Add(lastChange.PanaudotaProjekcija);
                }
                else
                {
                    bool radomeTinkamaProjekcija = false;
                    foreach (Projekcija p in db.Projekcijos)
                    {
                        if (!radomeTinkamaProjekcija)
                        {
                            if (p.Flag == 0)
                            {
                                if (p.Rezultatas == Tikslas)
                                {
                                    Change change = new Change();
                                    foreach (Projekcija pro in db.Projekcijos)
                                    {
                                        change.ProjekcijuSeniFlag.Add(pro.Flag);
                                    }
                                    change.SeniTikslai.AddRange(Tikslai);
                                    change.SeniKeliai.AddRange(db.Kelias);
                                    change.SeniFaktai.AddRange(db.Faktai);
                                    line += String.Format("Randame {0}. Nauji tikslai {1}.", p, db.CharListToString(p.Reikalavimai));
                                    Tikslai.RemoveAt(0);
                                    Tikslai.InsertRange(0, p.Reikalavimai);
                                    radomeTinkamaProjekcija = true;
                                    p.Flag = 1;

                                    change.IeskotasTikslas = Tikslas;
                                    change.NaujiTikslai.AddRange(Tikslai);
                                    change.PanaudotaProjekcija = p;
                                    foreach (Projekcija pro in db.Projekcijos)
                                    {
                                        change.ProjekcijuNaujiFlag.Add(pro.Flag);
                                    }
                                    change.Gylis = gylis;
                                    db.Changes.Push(change);

                                    gylis++;
                                }
                            }
                        }
                    }
                    if (!radomeTinkamaProjekcija)
                    {
                        line += String.Format("Nėra taisyklių jo išvedimui. Grįžtame, FAIL.");
                        gylis--;

                        Gryzti();
                    }
                }
                file.WriteLine(line);
            }
            file.WriteLine("");
            #endregion

            #region Rezultatai
            file.WriteLine("3 DALIS. Rezultatai");
            file.WriteLine(String.Format("  Tikslas {0} išvestas.", db.Tikslas));
            output = "R" + db.Kelias.ElementAt(0).Index.ToString();
            for (int i = 1; i < db.Kelias.Count; i++)
            {
                output += String.Format(", R{0}", db.Kelias.ElementAt(i).Index.ToString());
            }
            file.WriteLine(String.Format("  Kelias: {0}.", output));
            file.Close();
            #endregion
        }
    }
}
