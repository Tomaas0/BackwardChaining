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
        public static void Run(GDB db)
        {
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
            int gylis = 0;
            file.AutoFlush = true;//NUIMTI SITAAAAAAAA
            bool done = false;
            while (!done)
            {
                iCount++;
                String line = "";
                db.Tikslas = db.Tikslai.ElementAt(0);
                line += String.Format("  {0}) ", iCount.ToString());
                for(int i = 0; i < gylis; i++)
                {
                    line += "-";
                }
                line += String.Format("Tikslas {0}. ", db.Tikslas);

                List<char> laikinasList = new List<char>(db.Tikslai);
                laikinasList.RemoveAt(0);
                if (laikinasList.Contains(db.Tikslas))
                {
                    line += String.Format("Ciklas. Grįžtame, FAIL.");
                    gylis--;

                    List<int> remove = new List<int>();
                    for (int i = 0; i < db.Kelias.Count; i++)
                    {
                        if (db.KeliasWhenAdded.ElementAt(i) >= gylis)
                        {
                            remove.Add(i);
                        }
                    }
                    remove.Reverse();
                    foreach (int i in remove)
                    {
                        db.Kelias.RemoveAt(i);
                        db.KeliasWhenAdded.RemoveAt(i);
                    }

                    remove = new List<int>();
                    for (int i = 0; i < db.Faktai.Count; i++)
                    {
                        if (db.FaktaiWhenAdded.ElementAt(i) >= gylis)
                        {
                            remove.Add(i);
                        }
                    }
                    remove.Reverse();
                    foreach (int i in remove)
                    {
                        db.Faktai.RemoveAt(i);
                        db.FaktaiWhenAdded.RemoveAt(i);
                    }

                    db.Tikslai.RemoveAt(0);
                }
                else if (db.InitFaktai.Contains(db.Tikslas))
                {
                    line += String.Format("Faktas(duotas), nes faktai {0}.", db.FaktaiToString);
                    db.Tikslai.RemoveAt(0);
                        line += " Grįžtame, sėkmė.";
                        db.Faktai.Add(db.Tikslai.ElementAt(0));
                        db.FaktaiWhenAdded.Add(gylis);
                        gylis--;
                }
                else if (db.Faktai.Contains(db.Tikslas))
                {
                    line += String.Format("Faktas(dabar gautas). Faktai {0}.", db.FaktaiToString);
                    db.Tikslai.RemoveAt(0);
                    if(db.Tikslai.Count == 0)
                    {
                        done = true;
                    }
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
                                if (p.Rezultatas == db.Tikslas)
                                {
                                    line += String.Format("Randame {0}. Nauji tikslai {1}.", p, db.CharListToString(p.Reikalavimai));
                                    db.Tikslai.InsertRange(0, p.Reikalavimai);
                                    radomeTinkamaProjekcija = true;
                                    p.Flag = 1;
                                    db.Kelias.Add(p);
                                    db.KeliasWhenAdded.Add(gylis);
                                    gylis++;
                                }
                            }
                        }
                    }
                    if (!radomeTinkamaProjekcija)
                    {
                        line += String.Format("Nėra taisyklių jo išvedimui. Grįžtame, FAIL.");
                        gylis--;

                        List<int> remove = new List<int>();
                        for (int i = 0; i < db.Kelias.Count; i++)
                        {
                            if (db.KeliasWhenAdded.ElementAt(i) >= gylis)
                            {
                                remove.Add(i);
                            }
                        }
                        remove.Reverse();
                        foreach (int i in remove)
                        {
                            db.Kelias.RemoveAt(i);
                            db.KeliasWhenAdded.RemoveAt(i);
                        }

                        remove = new List<int>();
                        for (int i = 0; i < db.Faktai.Count; i++)
                        {
                            if (db.FaktaiWhenAdded.ElementAt(i) >= gylis)
                            {
                                remove.Add(i);
                            }
                        }
                        remove.Reverse();
                        foreach (int i in remove)
                        {
                            db.Faktai.RemoveAt(i);
                            db.FaktaiWhenAdded.RemoveAt(i);
                        }

                        db.Tikslai.RemoveAt(0);
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
