using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Timers;

namespace DemographyStatisticVizualizerGit
{
    class Program
    {
        public static int previouscount;
        public static int femalecount;
        static void Main(string[] args)
        {
            var setting = ConfigurationManager.AppSettings["ReaquestPeriodInSeconds"];
            Program p = new Program();
            Timer myTimer = new Timer();




            Console.WriteLine("Введите  общее количество людей :");
            previouscount = Convert.ToInt32(Console.ReadLine());
            Console.WriteLine("Женщин из них :");
            femalecount = Convert.ToInt32(Console.ReadLine());
            myTimer.Elapsed += new ElapsedEventHandler(p.Handler);
            myTimer.Interval = Convert.ToInt32(setting) * 1000;

            myTimer.Start();

            Console.ReadKey();

        }

        public List<StatisticEvent> GetStatisticsEvent()
        {
            return InterpretationReciveData(ReceiveStatisticData());
        }
        public Stream ReceiveStatisticData()
        {
            WebRequest request = WebRequest.Create("http://api.lod-misis.ru/testassignment");
            WebResponse response = request.GetResponse();
            Stream stream = response.GetResponseStream();
            return stream;
        }
        public List<StatisticEvent> InterpretationReciveData(Stream stream)
        {
            StreamReader reader = new StreamReader(stream);
            string[] splitedreader = reader.ReadToEnd().Split(new char[] { ':', '"', ';' }, StringSplitOptions.RemoveEmptyEntries);

            List<StatisticEvent> DataList = new List<StatisticEvent>();

            for (var i = 0; i < splitedreader.Length; i += 2)
            {

                DataList.Add(new StatisticEvent() { Gender = splitedreader[i], Condition = splitedreader[i + 1] });

            }
            return DataList;

        }
        public double[] AnalyzeStatisticEvent(List<StatisticEvent> DataList)
        {
            int PreviousPeopleCount = previouscount;
            int FemaleCount = femalecount;
            int MaleCount = previouscount - femalecount;
            double PercentCountMale = 0;
            double PercentCountFemale = 0;
            int MaleBirthCount = 0;
            int MaleDiedCount = 0;
            int FemaleBirthCount = 0;
            int FemaleDiedCount = 0;

            int BirthRateCount = 0;
            int DeathRateCount = 0;

            double BirthRateMinute = 1;
            double DeathRateMinute = 1;

            double ChildBirth = 0;
            double Mortality = 0;





            AnalyzeBornDiedMale(DataList, ref MaleBirthCount, ref MaleDiedCount, ref BirthRateCount, ref DeathRateCount);
            AnalyzeBornDiedFemale(DataList, ref FemaleBirthCount, ref FemaleDiedCount, ref BirthRateCount, ref DeathRateCount);

            GetFemaleCount(ref FemaleCount, FemaleBirthCount, FemaleDiedCount);
            GetMaleCount(ref  MaleCount, MaleBirthCount, MaleDiedCount);
            GetFullCountPeople(ref PreviousPeopleCount, FemaleCount, MaleCount);

            CulculatinOfPercentMale(ref PercentCountMale, MaleCount, PreviousPeopleCount);
            CulculatinOfPercentFemale(ref PercentCountFemale, FemaleCount, PreviousPeopleCount);

            GetBirthRateForMinute(ref BirthRateMinute, BirthRateCount);
            GetDeathRateForMinute(ref DeathRateMinute, BirthRateCount);

            CulculatinChildBirth(ref ChildBirth, BirthRateMinute);
            CulculationMortality(ref Mortality, DeathRateMinute);
            return new double[] { PreviousPeopleCount, PercentCountFemale, PercentCountMale,
                BirthRateMinute, DeathRateMinute, ChildBirth, Mortality };
        }
        public void CulculatinChildBirth(ref double ChildBirth, double BirthRateMinute)
        {
            ChildBirth = BirthRateMinute / 100;
        }
        public void CulculationMortality(ref double Mortality, double DeathRateMinute)
        {
            Mortality = DeathRateMinute / 100;
        }
        public void AnalyzeBornDiedMale(List<StatisticEvent> DataList, ref int MaleBirthCount, ref int MaleDiedCount, ref int BirthRateCount, ref int DeathRateCount)
        {
            MaleBirthCount = 0;
            MaleDiedCount = 0;

            foreach (var i in DataList)
            {
                if (i.Gender == "Male")
                {
                    if (i.Condition == "Born")
                    {
                        MaleBirthCount += 1;
                        BirthRateCount += 1;
                    }
                    else
                    {
                        MaleDiedCount += 1;
                        DeathRateCount += 1;
                    }
                }
            }
        }


        public void AnalyzeBornDiedFemale(List<StatisticEvent> DataList, ref int FemaleBirthCount, ref int FemaleDiedCount, ref int BirthRateCount, ref int DeathRateCount)
        {
            FemaleBirthCount = 0;
            FemaleDiedCount = 0;

            foreach (var i in DataList)
            {
                if (i.Gender == "Female")
                {
                    if (i.Condition == "Born")
                    {
                        FemaleBirthCount += 1;
                        BirthRateCount += 1;
                    }
                    else
                    {
                        FemaleDiedCount += 1;
                        DeathRateCount += 1;
                    }
                }

            }
        }
        public void GetMaleCount(ref int MaleCount, int MaleBirthCount, int MaleDiedCount)
        {
            MaleCount += MaleBirthCount - MaleDiedCount;
        }
        public void GetFemaleCount(ref int FemaleCount, int FemaleBirthCount, int FemaleDiedCount)
        {
            FemaleCount += FemaleCount + FemaleBirthCount - FemaleDiedCount;
        }
        public void GetFullCountPeople(ref int PreviousPeopleCount, int FemaleCount, int MaleCount)
        {
            PreviousPeopleCount = FemaleCount + MaleCount;
        }
        public void CulculatinOfPercentMale(ref double PercentCountMale, int MaleCount, int PreviousPeopleCount)
        {
            PercentCountMale = MaleCount * 100 / PreviousPeopleCount;
        }
        public void CulculatinOfPercentFemale(ref double PercentCountFemale, int FemaleCount, int PreviousPeopleCount)
        {
            PercentCountFemale = FemaleCount * 100 / PreviousPeopleCount;
        }
        public void GetBirthRateForMinute(ref double BirthRateMinute, int BirthRateCount)
        {

            BirthRateMinute = BirthRateCount * 6;
        }
        public void GetDeathRateForMinute(ref double DeathRateMinute, int DeathRateCount)
        {
            DeathRateMinute = DeathRateCount * 6;
        }



        public void Handler(object sender, EventArgs e)
        {

            PrintOutStatistic(GetStatisticsEvent());
        }
        public void PrintOutStatistic(List<StatisticEvent> DataList)
        {
            double[] array;
            Console.Clear();
            array = AnalyzeStatisticEvent(DataList);
            Console.WriteLine("Текущее количество жителей: " + array[0]);
            Console.WriteLine("Процент женщин :" + array[1]);
            Console.WriteLine("Процент мужчин :" + array[2]);
            Console.WriteLine("Частота родов :" + array[3]);
            Console.WriteLine("Частота смертей :" + array[4]);
            Console.WriteLine("Рождаемость :" + array[5]);
            Console.WriteLine("Смертность :" + array[6]);

        }
    }



    public class StatisticEvent
    {
        public string Gender { get; set; }
        public string Condition { get; set; }


    }
}

