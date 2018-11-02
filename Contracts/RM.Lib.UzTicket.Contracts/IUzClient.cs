using System;
using System.Threading.Tasks;
using RM.Lib.UzTicket.Contracts.DataContracts;

namespace RM.Lib.UzTicket.Contracts
{
    public interface IUzClient
    {
	    event EventHandler<ScanEventArgs> ScanEvent;

	    //void StartScan();

	    //void StopScan();

		Task<Station[]> GetStationsAsync(string name);

	    Task<Station> GetFirstStationAsync(string name);

	    string[] GetScanStatus(long? callback);

	    Task ResetScan();
    }
}