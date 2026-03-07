#pragma warning disable 0414, 1591
#region Designer generated code

using grpc = global::Grpc.Core;

namespace Game.Network
{
    public static partial class UserService
    {
        static readonly string __ServiceName = "user.UserService";

        // Marshallers の定義
        static readonly grpc::Marshaller<global::Game.Network.CreateUserRequest> __Marshaller_user_CreateUserRequest = grpc::Marshallers.Create((arg) => global::Google.Protobuf.MessageExtensions.ToByteArray(arg), global::Game.Network.CreateUserRequest.Parser.ParseFrom);
        static readonly grpc::Marshaller<global::Game.Network.UserResponse> __Marshaller_user_UserResponse = grpc::Marshallers.Create((arg) => global::Google.Protobuf.MessageExtensions.ToByteArray(arg), global::Game.Network.UserResponse.Parser.ParseFrom);
        static readonly grpc::Marshaller<global::Game.Network.GetUserRequest> __Marshaller_user_GetUserRequest = grpc::Marshallers.Create((arg) => global::Google.Protobuf.MessageExtensions.ToByteArray(arg), global::Game.Network.GetUserRequest.Parser.ParseFrom);
        static readonly grpc::Marshaller<global::Game.Network.LoginRequest> __Marshaller_user_LoginRequest = grpc::Marshallers.Create((arg) => global::Google.Protobuf.MessageExtensions.ToByteArray(arg), global::Game.Network.LoginRequest.Parser.ParseFrom);
        static readonly grpc::Marshaller<global::Game.Network.ListUsersRequest> __Marshaller_user_ListUsersRequest = grpc::Marshallers.Create((arg) => global::Google.Protobuf.MessageExtensions.ToByteArray(arg), global::Game.Network.ListUsersRequest.Parser.ParseFrom);
        static readonly grpc::Marshaller<global::Game.Network.ListUsersResponse> __Marshaller_user_ListUsersResponse = grpc::Marshallers.Create((arg) => global::Google.Protobuf.MessageExtensions.ToByteArray(arg), global::Game.Network.ListUsersResponse.Parser.ParseFrom);

        // Methods の定義
        static readonly grpc::Method<global::Game.Network.CreateUserRequest, global::Game.Network.UserResponse> __Method_CreateUser = new grpc::Method<global::Game.Network.CreateUserRequest, global::Game.Network.UserResponse>(grpc::MethodType.Unary, __ServiceName, "CreateUser", __Marshaller_user_CreateUserRequest, __Marshaller_user_UserResponse);
        static readonly grpc::Method<global::Game.Network.GetUserRequest, global::Game.Network.UserResponse> __Method_GetUser = new grpc::Method<global::Game.Network.GetUserRequest, global::Game.Network.UserResponse>(grpc::MethodType.Unary, __ServiceName, "GetUser", __Marshaller_user_GetUserRequest, __Marshaller_user_UserResponse);
        static readonly grpc::Method<global::Game.Network.LoginRequest, global::Game.Network.UserResponse> __Method_Login = new grpc::Method<global::Game.Network.LoginRequest, global::Game.Network.UserResponse>(grpc::MethodType.Unary, __ServiceName, "Login", __Marshaller_user_LoginRequest, __Marshaller_user_UserResponse);
        static readonly grpc::Method<global::Game.Network.ListUsersRequest, global::Game.Network.ListUsersResponse> __Method_ListUsers = new grpc::Method<global::Game.Network.ListUsersRequest, global::Game.Network.ListUsersResponse>(grpc::MethodType.Unary, __ServiceName, "ListUsers", __Marshaller_user_ListUsersRequest, __Marshaller_user_ListUsersResponse);

        public partial class UserServiceClient : grpc::ClientBase<UserServiceClient>
        {
            public UserServiceClient(grpc::ChannelBase channel) : base(channel) { }
            public UserServiceClient(grpc::CallInvoker callInvoker) : base(callInvoker) { }
            protected UserServiceClient() : base() { }
            protected UserServiceClient(ClientBaseConfiguration configuration) : base(configuration) { }

            // 各RPCの Async メソッド群
            public virtual grpc::AsyncUnaryCall<global::Game.Network.UserResponse> CreateUserAsync(global::Game.Network.CreateUserRequest request, grpc::Metadata headers = null, global::System.DateTime? deadline = null, global::System.Threading.CancellationToken cancellationToken = default(global::System.Threading.CancellationToken))
            { return CallInvoker.AsyncUnaryCall(__Method_CreateUser, null, new grpc::CallOptions(headers, deadline, cancellationToken), request); }

            public virtual grpc::AsyncUnaryCall<global::Game.Network.UserResponse> GetUserAsync(global::Game.Network.GetUserRequest request, grpc::Metadata headers = null, global::System.DateTime? deadline = null, global::System.Threading.CancellationToken cancellationToken = default(global::System.Threading.CancellationToken))
            { return CallInvoker.AsyncUnaryCall(__Method_GetUser, null, new grpc::CallOptions(headers, deadline, cancellationToken), request); }

            public virtual grpc::AsyncUnaryCall<global::Game.Network.UserResponse> LoginAsync(global::Game.Network.LoginRequest request, grpc::Metadata headers = null, global::System.DateTime? deadline = null, global::System.Threading.CancellationToken cancellationToken = default(global::System.Threading.CancellationToken))
            { return CallInvoker.AsyncUnaryCall(__Method_Login, null, new grpc::CallOptions(headers, deadline, cancellationToken), request); }

            public virtual grpc::AsyncUnaryCall<global::Game.Network.ListUsersResponse> ListUsersAsync(global::Game.Network.ListUsersRequest request, grpc::Metadata headers = null, global::System.DateTime? deadline = null, global::System.Threading.CancellationToken cancellationToken = default(global::System.Threading.CancellationToken))
            { return CallInvoker.AsyncUnaryCall(__Method_ListUsers, null, new grpc::CallOptions(headers, deadline, cancellationToken), request); }

            protected override UserServiceClient NewInstance(ClientBaseConfiguration configuration)
            { return new UserServiceClient(configuration); }
        }
    }
}
#endregion