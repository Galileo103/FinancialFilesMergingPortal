using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;
using System.Web;


namespace FinancialFilesMergingPortal.Services
{
    [ServiceContract]
    public interface IFinancialFilesService
    {
        [OperationContract]
        string SendData(string userName, List<byte[]> files, List<string> filesNames);

        byte[] ConverToBytes(HttpPostedFileBase file);

        byte[] numberToByteArray(long src);

        byte[] stringToByteArray(string src);

        int getCreationDateTime();

        bool isTheSameHeader(List<byte[]> files);

        void setInputFinancialFile(List<byte[]> files, List<string> filesNames);

        void setInputFinancialFilesBatch(string userName, string status, string errorDescription);

        string MergedFiles(List<byte[]> files);

        string MergedFile(byte[] originalFile, Dictionary<int, long> values, int DrecordsCount, long DrecordsAmount, int CrecordsCount, long CrecordsAmount);


    }
}
