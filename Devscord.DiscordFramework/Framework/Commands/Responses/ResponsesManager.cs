﻿using System;
using Devscord.DiscordFramework.Middlewares.Contexts;
using System.Collections.Generic;
using System.Globalization;

namespace Devscord.DiscordFramework.Framework.Commands.Responses
{
    public static class ResponsesManager
    {
        public static string RoleAddedToUser(this ResponsesService responsesService, Contexts contexts, string role)
        {
            return responsesService.ProcessResponse("RoleAddedToUser", contexts, new KeyValuePair<string, string>("role", role));
        }

        public static string RoleRemovedFromUser(this ResponsesService responsesService, Contexts contexts, string role)
        {
            return responsesService.ProcessResponse("RoleRemovedFromUser", contexts, new KeyValuePair<string, string>("role", role));
        }        
        
        public static string RoleNotFoundInUser(this ResponsesService responsesService, Contexts contexts, string role)
        {
            return responsesService.ProcessResponse("RoleNotFoundInUser", contexts, new KeyValuePair<string, string>("role", role));
        }        
        
        public static string RoleNotFoundOrIsNotSafe(this ResponsesService responsesService, Contexts contexts, string role)
        {
            return responsesService.ProcessResponse("RoleNotFoundOrIsNotSafe", contexts, new KeyValuePair<string, string>("role", role));
        }        

        public static string RoleIsInUserAlready(this ResponsesService responsesService, Contexts contexts, string role)
        {
            return responsesService.ProcessResponse("RoleIsInUserAlready", contexts, new KeyValuePair<string, string>("role", role));
        }

        public static string SpamAlertRecognized(this ResponsesService responsesService, Contexts contexts)
        {
            return responsesService.ProcessResponse("SpamAlertRecognized", contexts);
        }        
        
        public static string SpamAlertUserIsMuted(this ResponsesService responsesService, Contexts contexts)
        {
            return responsesService.ProcessResponse("SpamAlertUserIsMuted", contexts);
        }
        
        public static string NewUserArrived(this ResponsesService responsesService, Contexts contexts)
        {
            return responsesService.ProcessResponse("NewUserArrived", contexts);
        }

        public static string CurrentVersion(this ResponsesService responsesService, Contexts contexts, string version)
        {
            return responsesService.ProcessResponse("CurrentVersion", contexts, new KeyValuePair<string, string>("version", version));
        }

        public static string PrintHelp(this ResponsesService responsesService, string help)
        {
            return responsesService.ProcessResponse("PrintHelp", new KeyValuePair<string, string>("help", help));
        }

        public static string UserIsNotAdmin(this ResponsesService responsesService)
        {
            return responsesService.ProcessResponse("UserIsNotAdmin");
        }

        public static string UserDidntMentionedAnyUserToMute(this ResponsesService responsesService)
        {
            return responsesService.ProcessResponse("UserDidntMentionedAnyUserToMute");
        }  
        
        public static string UserNotFound(this ResponsesService responsesService, string userMention)
        {
            return responsesService.ProcessResponse("UserNotFound", new KeyValuePair<string, string>("user", userMention));
        }

        public static string RoleNotFound(this ResponsesService responsesService, string roleName)
        {
            return responsesService.ProcessResponse("RoleNotFound", new KeyValuePair<string, string>("role", roleName));
        }

        public static string MutedUser(this ResponsesService responsesService, UserContext mutedUser, DateTime timeEnd)
        {
            return responsesService.ProcessResponse("MutedUser", 
                new KeyValuePair<string, string>("user", mutedUser.Name),
                new KeyValuePair<string, string>("timeEnd", timeEnd.ToString(CultureInfo.InvariantCulture)));
        }

        public static string UnmutedUser(this ResponsesService responsesService, UserContext unmutedUser)
        {
            return responsesService.ProcessResponse("UnmutedUser", new KeyValuePair<string, string>("user", unmutedUser.Name));
        }
    }
}
