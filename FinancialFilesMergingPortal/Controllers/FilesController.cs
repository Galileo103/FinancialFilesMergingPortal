﻿using FinancialFilesMergingPortal.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace FinancialFilesMergingPortal.Controllers
{
    public class FilesController : Controller
    {

        FileMergeServiceReference.FinancialFilesServiceClient client = new FileMergeServiceReference.FinancialFilesServiceClient("BasicHttpBinding_IFinancialFilesService");

        
        //
        // GET: /Files/
        [HttpGet]
        public ActionResult Upload()
        {
            return View();
        }


        [HttpPost]
        public ActionResult Upload(List<HttpPostedFileBase> files)
        {
            var path = "";
            

            List<byte[]> allFiles = new List<byte[]>();
            List<string> allFilesNames = new List<string>();
            foreach (HttpPostedFileBase file in files)
            {
                if (file != null)
                {
                    if (file.ContentLength > 0)
                    {
                        byte[] tmpFile = ConverToBytes(file);
                        allFiles.Add(tmpFile);
                        allFilesNames.Add(file.FileName);
                    }
                }
            }


            string userName = TempData["userName"] as string;

            //string w = client.SendData("Galal", allFiles, allFilesNames);
            //This problem Solved by add .ToArray()
            string w = client.SendData(userName, allFiles.ToArray(), allFilesNames.ToArray());

            MergedFiles(allFiles);
            //setInputFinancialFile(allFiles, allFilesNames);
            //we will use this in the WCF
            bool x = isTheSameHeader(allFiles);
            ViewBag.UploadSuccess = true;


            return View();
        }

        public static byte[] ConverToBytes(HttpPostedFileBase file)
        {
            var length = file.InputStream.Length; //Length: 103050706
            byte[] fileData = null;
            using (var binaryReader = new BinaryReader(file.InputStream))
            {
                fileData = binaryReader.ReadBytes(file.ContentLength);
            }
            return fileData;
        }

        public static byte[] numberToByteArray(long src)
        {
            return BitConverter.GetBytes(src);
        }

        public static byte[] stringToByteArray(string src)
        {
            return System.Text.Encoding.ASCII.GetBytes(src as string);
        }

        public static bool isTheSameHeader(List<byte[]> files)
        {
            int headerSize = 17;
            byte[] firstHeaderContent = files[0].Take(headerSize).ToArray();
            int len = files.Count;
            for (int i = 1; i < len; i++)
            {
                byte[] otherHeaderContent = files[i].Take(headerSize).ToArray();
                if (!firstHeaderContent.SequenceEqual(otherHeaderContent))
                {
                    return false;
                }
            }
            return true;
        }

        public static void setInputFinancialFile(List<byte[]> files, List<string> filesNames)
        {

            LinqToSQLDataContext db = new LinqToSQLDataContext();

            
            int len = files.Count;
            int headerPart1 = 2;
            int headerPart2 = 7;
            int headerPart3 = 8;

            int trailerpart1 = 2;
            int trailerpart2 = 7;
            int trailerpart3 = 17;
            int trailerpart4 = 2;
            int trailerpart5 = 17;
            for (int i = 0; i < len; i++)
            {

                //Create new InputFinancialFile Object
                InputFinancialFile newFile = new InputFinancialFile();
            
                var first = files[i].Take(headerPart1).ToArray();
                var second = files[i].Skip(headerPart1).ToArray();

                //var str = System.Text.Encoding.Default.GetString(first);
                //will skip the first 2 byte

                first = second.Take(headerPart2).ToArray();
                second = second.Skip(headerPart2).ToArray();
                newFile.FUN = BitConverter.ToInt32(first, 0);


                first = second.Take(headerPart3).ToArray();
                second = second.Skip(headerPart3).ToArray();
                newFile.DateTime = BitConverter.ToInt32(first, 0);


                // the total size - the size of header
                int bodySkip = files[i].Length - trailerpart1 - trailerpart2 - trailerpart3 - trailerpart4 - trailerpart5;
                first = files[i].Take(bodySkip).ToArray();
                second = files[i].Skip(bodySkip).ToArray();

                //will skip the all content

                first = second.Take(trailerpart1).ToArray();
                second = second.Skip(trailerpart1).ToArray();
                // will skip the first 2 bytes

                first = second.Take(trailerpart2).ToArray();
                second = second.Skip(trailerpart2).ToArray();
                newFile.TotalCreditCount = BitConverter.ToInt32(first, 0);

                first = second.Take(trailerpart3).ToArray();
                second = second.Skip(trailerpart3).ToArray();
                newFile.TotalCreditAmount = BitConverter.ToInt32(first, 0);

                first = second.Take(trailerpart4).ToArray();
                second = second.Skip(trailerpart4).ToArray();
                newFile.TotalDebitCount = BitConverter.ToInt16(first, 0);

                first = second.Take(trailerpart5).ToArray();
                second = second.Skip(trailerpart5).ToArray();
                newFile.TotalDebitAmount = BitConverter.ToInt32(first, 0);


                newFile.FileName = filesNames[i];

                newFile.PhysicalLocation = "";  // until know


                //Add new Employee to database
                db.InputFinancialFiles.InsertOnSubmit(newFile);

                //Save changes to Database.
                db.SubmitChanges();

            }
        }

        public static void MergedFiles(List<byte[]> files)
        {
            int len = files.Count;
            int headerPart1 = 2;
            int headerPart2 = 7;
            int headerPart3 = 8;
            
            int bodyPart1 = 2;
            int bodyPart2 = 7;
            int bodyPart3 = 4;
            int bodyPart4 = 2;
            int bodyPart5 = 17;

            int trailerpart1 = 2;
            int trailerpart2 = 7;
            int trailerpart3 = 17;
            int trailerpart4 = 2;
            int trailerpart5 = 17;

            var newValues = new Dictionary<int, long>();
            int recordNumber = 0;
            long amount;

            int DrecordsCount = 0;
            int CrecordsCount = 0;
            long DrecordsAmount = 0;
            long CrecordsAmount = 0;

            long tmpOut;

            for (int i = 0; i < len; i++)
            {

            
                var first = files[i].Take(headerPart1 + headerPart2 + headerPart3).ToArray();
                var second = files[i].Skip(headerPart1 + headerPart2 + headerPart3).ToArray();
                //will skip the file header

                string loop = "FR";
                while(loop == "FR")
                {
                    first = second.Take(bodyPart1).ToArray();
                    second = second.Skip(bodyPart1).ToArray();
                    if(loop != System.Text.Encoding.Default.GetString(first))
                    {
                        loop = System.Text.Encoding.Default.GetString(first);
                        continue;
                    }

                    first = second.Take(bodyPart2).ToArray();
                    second = second.Skip(bodyPart2).ToArray();
                    recordNumber = BitConverter.ToInt32(first, 0);

                    first = second.Take(bodyPart3 + bodyPart4).ToArray();
                    second = second.Skip(bodyPart3 + bodyPart4).ToArray();
                    //will skip the Bank Code and Transaction Type

                    first = second.Take(bodyPart5).ToArray();
                    second = second.Skip(bodyPart5).ToArray();
                    amount = BitConverter.ToInt32(first, 0);

                    if (!newValues.TryGetValue(recordNumber, out tmpOut))
                    {
                        newValues.Add(recordNumber, amount);
                    }
                    else
                    {
                        newValues[recordNumber] += amount;
                    }

                }

                first = second.Take(trailerpart2).ToArray();
                second = second.Skip(trailerpart2).ToArray();
                if (0 == i)
                {
                    DrecordsCount += BitConverter.ToInt32(first, 0);
                }

                first = second.Take(trailerpart3).ToArray();
                second = second.Skip(trailerpart3).ToArray();
                DrecordsAmount += BitConverter.ToInt32(first, 0);


                first = second.Take(trailerpart4).ToArray();
                second = second.Skip(trailerpart4).ToArray();
                if (0 == i)
                {
                    CrecordsCount += BitConverter.ToInt16(first, 0);
                }

                first = second.Take(trailerpart5).ToArray();
                second = second.Skip(trailerpart5).ToArray();
                CrecordsAmount += BitConverter.ToInt32(first, 0);


            }

            MergedFile(files[0], newValues, DrecordsCount, DrecordsAmount, CrecordsCount, CrecordsAmount);
            
        }

        public static void MergedFile(byte[] originalFile, Dictionary<int, long> values, int DrecordsCount, long DrecordsAmount, int CrecordsCount, long CrecordsAmount)
        {
            int headerPart1 = 2;
            int headerPart2 = 7;
            int headerPart3 = 8;

            int bodyPart1 = 2;
            int bodyPart2 = 7;
            int bodyPart3 = 4;
            int bodyPart4 = 2;
            int bodyPart5 = 17;

            int trailerpart1 = 2;
            int trailerpart2 = 7;
            int trailerpart3 = 17;
            int trailerpart4 = 2;
            int trailerpart5 = 17;

            using (System.IO.FileStream file =
            new System.IO.FileStream(@"C:\Users\Galileo\Documents\visual studio 2013\Projects\FinancialFilesMergingPortal\FinancialFilesMergingPortal\Mfile.txt", System.IO.FileMode.Create))
            {
                var first = originalFile.Take(headerPart1).ToArray();
                var second = originalFile.Skip(headerPart1).ToArray();
                file.Write(first, 0, headerPart1);


                first = second.Take(headerPart2).ToArray();
                second = second.Skip(headerPart2).ToArray();
                file.Write(first, 0, headerPart2);

                first = second.Take(headerPart3).ToArray();
                second = second.Skip(headerPart3).ToArray();
                file.Write(first, 0, headerPart3);

                string loop = "FR";
                while (loop == "FR")
                {
                    first = second.Take(bodyPart1).ToArray();
                    second = second.Skip(bodyPart1).ToArray();
                    if (loop != System.Text.Encoding.Default.GetString(first))
                    {
                        loop = System.Text.Encoding.Default.GetString(first);
                        continue;
                    }

                    file.Write(first, 0, bodyPart1);

                    first = second.Take(bodyPart2).ToArray();
                    second = second.Skip(bodyPart2).ToArray();
                    int key = BitConverter.ToInt32(first, 0);
                    file.Write(first, 0, bodyPart2);

                    first = second.Take(bodyPart3).ToArray();
                    second = second.Skip(bodyPart3).ToArray();
                    file.Write(first, 0, bodyPart3);

                    first = second.Take(bodyPart4).ToArray();
                    second = second.Skip(bodyPart4).ToArray();
                    file.Write(first, 0, bodyPart4);

                    // for skip bits only
                    first = second.Take(bodyPart5).ToArray();
                    second = second.Skip(bodyPart5).ToArray();

                    first = numberToByteArray(values[key]); // the new value
                    System.Array.Resize(ref first, 17);
                    file.Write(first, 0, bodyPart5);
                }


                // Write the Trailer

                file.Write(stringToByteArray("FT"), 0, trailerpart1);

                byte[] BytesToWrite = numberToByteArray(DrecordsCount);
                file.Write(BytesToWrite, 0, trailerpart2);

                BytesToWrite = numberToByteArray(DrecordsAmount);
                System.Array.Resize(ref BytesToWrite, 17);
                file.Write(BytesToWrite, 0, trailerpart3);

                BytesToWrite = numberToByteArray(CrecordsCount);
                file.Write(BytesToWrite, 0, trailerpart4);

                BytesToWrite = numberToByteArray(CrecordsAmount);
                System.Array.Resize(ref BytesToWrite, 17);
                file.Write(BytesToWrite, 0, trailerpart5);
            }

        }

	}
}