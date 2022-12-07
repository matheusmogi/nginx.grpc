using Grpc.Core;

namespace NginxPoC.Services;

public class GreeterService : Greeter.GreeterBase
{
    public override Task<HelloReply> UnaryCall(HelloRequest request, ServerCallContext context)
    {
        var response = new HelloReply
        {
            Message = "Hello " + request.Name
        };
        return Task.FromResult(response);
    }

    public override async Task StreamingFromServer(HelloRequest request, IServerStreamWriter<HelloReply> responseStream, ServerCallContext context)
    {
        for (var i = 0; i < 5; i++)
        {
            await responseStream.WriteAsync(new HelloReply
            {
                Message = "Hello " + request.Name
            });

            await Task.Delay(TimeSpan.FromSeconds(1));
        }
    }

    public override async Task<HelloReply> StreamingFromClient(IAsyncStreamReader<HelloRequest> requestStream, ServerCallContext context)
    {
        var message = new HelloRequest();
        while (await requestStream.MoveNext())
        {
            message = requestStream.Current;
        }

        return new HelloReply
        {
            Message = "Hello " + message.Name
        };
    }

    public override async Task StreamingBothWays(IAsyncStreamReader<HelloRequest> requestStream,
        IServerStreamWriter<HelloReply> responseStream, ServerCallContext context)
    {
        var internalMessage = new HelloRequest();
        // Read requests in a background task.
        var readTask = Task.Run(async () =>
        {
            await foreach (var message in requestStream.ReadAllAsync())
            {
                internalMessage = message;
            }
        });
    
        // Send responses until the client signals that it is complete.
        while (!readTask.IsCompleted)
        {
            await responseStream.WriteAsync(new HelloReply
            {
                Message = "Hello " + internalMessage.Name
            });
            await Task.Delay(TimeSpan.FromSeconds(1), context.CancellationToken);
        }
    }
}