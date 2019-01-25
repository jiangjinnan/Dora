using GraphQL.Resolvers;
using GraphQL.Subscription;
using GraphQL.Types;
using System;
using System.Linq;
using System.Reactive.Linq;
using System.Security.Claims;

namespace GraphQL.Samples.Schemas.Chat
{
    public class ChatSubscriptions : ObjectGraphType<object>
    {
        private readonly IChat _chat;

        public ChatSubscriptions(IChat chat)
        {
            _chat = chat;
            AddField(new EventStreamFieldType
            {
                Name = "messageAdded",
                Type = typeof(MessageType),
                Resolver = new FuncFieldResolver<Message>(ResolveMessage)
            });

            AddField(new EventStreamFieldType
            {
                Name = "messageAddedByUser",
                Arguments = new QueryArguments(
                    new QueryArgument<NonNullGraphType<StringGraphType>> {Name = "id"}
                ),
                Type = typeof(MessageType),
                Resolver = new FuncFieldResolver<Message>(ResolveMessage)
            });
        }


        private Message ResolveMessage(ResolveFieldContext context)
        {
            var message = context.Source as Message;

            return message;
        }

    }
}