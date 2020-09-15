﻿using MediatR;
using SK.Application.Common.Exceptions;
using SK.Application.Common.Interfaces;
using SK.Application.Common.Models;
using SK.Domain.Entities;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace SK.Application.User.Commands.DeleteUser
{
    public class DeleteUserCommandHandler : IRequestHandler<DeleteUserCommand, Result>
    {
        private readonly IIdentityService _identityService;
        private readonly ICurrentUserService _currentUserService;

        public DeleteUserCommandHandler(IIdentityService identityService, ICurrentUserService currentUserService)
        {
            _identityService = identityService;
            _currentUserService = currentUserService;
        }

        public async Task<Result> Handle(DeleteUserCommand request, CancellationToken cancellationToken)
        {
            var user = await _identityService.GetUserByUsernameAsync(request.Username) ?? throw new NotFoundException(nameof(AppUser), request.Username);

            if (user.UserName != _currentUserService.Username)
            {
                throw new RestException(HttpStatusCode.Unauthorized, new { Username = "Not authorized username." });
            }

            var result = await _identityService.DeleteUserAsync(user.UserName);

            return result;
        }
    }
}
