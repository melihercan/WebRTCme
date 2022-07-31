using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace WebRTCme
{
    public interface IRTCStatsReport : IDisposable // INativeObject
    {
        /////////////////////////////// TODO; CHANGE
        /// Call to this function returns RTCStatsReport but itr does not reveal yhe following entries directly.
        /// Instead 'forEach' function should be called with callbacks
        /// interface RTCStatsReport {
        // forEach(callbackfn: (value: any, key: string, parent: RTCStatsReport) => void, thisArg?: any): void;
        //}
        /////// I dont currently support callbacks from JS, only events. Need to add callback mechanism, 
        ///this will be similar to event handling without calling OnEvent... JS stuff
        ///
   //     ///////// 			
   //     let cameraStats = await _pc.getStats();//// this.getSendTransportLocalStats(); 
   //     let statsString = '';
   //     cameraStats?.forEach(res => {
   //         statsString += '<h3>Report type=';
   //         statsString += res.type;
   //         statsString += '</h3>\n';
   //         statsString += 'id ' + res.id + '<br>\n';
   //         statsString += 'time ' + res.timestamp + '<br>\n';
   //         Object.keys(res).forEach(k => {
   //             if (k !== 'timestamp' && k !== 'type' && k !== 'id')
   //             {
   //                 statsString += k + ': ' + res[k] + '<br>\n';
   //             }
   //         });
   //     });
			//console.log(statsString);


        ///
        ///////    CURRENTLY HACKED JsInterop.js and add a function that takes RTCPeerConnection as object and returns 
        ///ststistics in string.
        ////////////
        ///////













    string Id { get; set; }

        double Timestamp { get; set; }

        RTCStatsType Type { get; set; }
    }
}
