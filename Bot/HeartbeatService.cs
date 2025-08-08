
using System.Text.Json;

namespace FenDB.Bot;

public class HeartbeatService{
    public void Start(GatewayConnection connection){
        _ = Task.Run(async () =>{
            while(true){
                await Task.Delay(30000);
                var hearbeat = JsonSerializer.Serialize(new{op=1,d = (object?)null});
                await connection.SendAsync(hearbeat);
                Console.WriteLine("Send Heartbeat");
            }
        });
    }
}