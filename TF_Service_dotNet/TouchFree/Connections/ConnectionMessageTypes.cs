using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Ultraleap.TouchFree.Library.Configuration;
using Ultraleap.TouchFree.Library.Connections.DiagnosticApi;

namespace Ultraleap.TouchFree.Library.Connections;

public enum TrackingServiceState
{
    UNAVAILABLE,
    NO_CAMERA,
    CONNECTED
}

public enum BinaryMessageType
{
    Hand_Data = 1
}

public enum ConfigurationState
{
    NOT_LOADED,
    LOADED,
    ERRORED
}

[Serializable]
public readonly record struct ConfigState(string requestID, InteractionConfig interaction, PhysicalConfig physical);

[Serializable]
public readonly record struct ServiceStatus(string requestID,
    TrackingServiceState trackingServiceState,
    ConfigurationState configurationState,
    string serviceVersion,
    string trackingVersion,
    string cameraSerial,
    string cameraFirmwareVersion)
{
    public static ServiceStatus FromDApiTypes(string requestId,
        TrackingServiceState trackingServiceState,
        ConfigurationState configurationState,
        string serviceVersion,
        in ApiInfo? apiInfo,
        in DeviceInfo? deviceInfo) =>
        new(requestId,
            trackingServiceState,
            configurationState,
            serviceVersion,
            apiInfo.GetValueOrDefault().ServiceVersion ?? "Tracking not connected",
            deviceInfo.GetValueOrDefault().Serial ?? "Device not connected",
            deviceInfo.GetValueOrDefault().Firmware ?? "Device not connected");
}

[Serializable]
public readonly record struct HandShakeResponse(string requestID, string status, string message,
    string originalRequest, string touchFreeVersion, string apiVersion);

[Serializable]
public readonly record struct ResponseToClient(string requestID, string status, string message, string originalRequest);

[Serializable]
public record struct MaskingData(double lower, double upper, double right, double left)
{
    public static explicit operator MaskingData(in Configuration.MaskingData other) => new()
    {
        left = other.Left,
        right = other.Right,
        upper = other.Upper,
        lower = other.Lower
    };

    public static explicit operator Configuration.MaskingData(in MaskingData data) => new()
    {
        Left = data.left,
        Right = data.right,
        Lower = data.lower,
        Upper = data.upper
    };
}

[Serializable]
public readonly record struct TrackingApiState(
    string requestID,
    in SuccessWrapper<MaskingData?>? mask,
    in SuccessWrapper<bool?>? allowImages,
    in SuccessWrapper<bool?>? cameraReversed,
    in SuccessWrapper<bool?>? analyticsEnabled);

public readonly record struct SuccessWrapper<T>(bool succeeded, string msg, in T? content);

public readonly record struct IncomingRequest(ActionCode ActionCode, string Content)
{
    public Result<IncomingRequestWithId> DeserializeAndValidateRequestId()
    {
        var contentObj = JsonConvert.DeserializeObject<JObject>(Content);
        if (contentObj == null) return new Error("Deserializing request content failed: returned null");
            
        var request = this; // Lambda cannot capture "this" in structs, need to copy to a local
        return MessageValidation.ValidateRequestId(contentObj)
            .Map(id =>  new IncomingRequestWithId(request.ActionCode, contentObj, id, request.Content));
    }
}
public readonly record struct IncomingRequestWithId(ActionCode ActionCode, JObject ContentRoot, string RequestId, string OriginalContent);