﻿using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Localization;
using Moq;
using NUnit.Framework;
using SK.API.Services;
using SK.Application.Common.Exceptions;
using SK.Application.Common.Interfaces;
using SK.Application.Common.Models;
using SK.Application.Common.Resources.Users;
using SK.Application.User.Commands.DeleteUser;
using SK.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SK.Application.UnitTests.User.Commands
{
    public class DeleteUserCommandTest
    {
        private const string userName = "User";

        private readonly Mock<IIdentityService> identityService;
        private readonly ICurrentUserService currentUserService;
        private readonly Mock<IStringLocalizer<UsersResource>> stringLocalizer;

        private readonly AppUser user;

        public DeleteUserCommandTest()
        {
            identityService = new Mock<IIdentityService>();
            currentUserService = Mock.Of<ICurrentUserService>(x => x.Username == userName);
            stringLocalizer = new Mock<IStringLocalizer<UsersResource>>();

            user = new AppUser { UserName = userName };
        }

        [Test]
        public async Task ShouldCallHandle()
        {
            //Arrange
            identityService.Setup(x => x.GetUserByUsernameAsync(userName)).Returns(Task.FromResult(user));
            identityService.Setup(x => x.DeleteUserAsync(userName)).Returns(Task.FromResult(Result.Success()));

            DeleteUserCommandHandler deleteUserCommandHandler = new DeleteUserCommandHandler(identityService.Object, currentUserService, stringLocalizer.Object);
            DeleteUserCommand deleteUserCommand = new DeleteUserCommand(userName);

            //Act
            var result = await deleteUserCommandHandler.Handle(deleteUserCommand, new CancellationToken());

            //Assert
            result.Succeeded.Should().BeTrue();
        }

        [Test]
        public void ShouldNotCallHandleIfUserNotExist()
        {
            //Arrange
            identityService.Setup(x => x.GetUserByUsernameAsync(userName)).Returns(Task.FromResult((AppUser)null));

            DeleteUserCommandHandler deleteUserCommandHandler = new DeleteUserCommandHandler(identityService.Object, currentUserService, stringLocalizer.Object);
            DeleteUserCommand deleteUserCommand = new DeleteUserCommand(userName);

            //Act
            Func<Task> act = async () => await deleteUserCommandHandler.Handle(deleteUserCommand, new CancellationToken());

            //Assert
            act.Should().Throw<NotFoundException>();
        }

        [Test]
        public void ShouldNotCallHandleIfCurrentUserNotMatch()
        {
            //Arrange
            user.UserName = It.IsAny<string>();
            identityService.Setup(x => x.GetUserByUsernameAsync(userName)).Returns(Task.FromResult(user));
            identityService.Setup(x => x.DeleteUserAsync(userName)).Returns(Task.FromResult(Result.Success()));

            DeleteUserCommandHandler deleteUserCommandHandler = new DeleteUserCommandHandler(identityService.Object, currentUserService, stringLocalizer.Object);
            DeleteUserCommand deleteUserCommand = new DeleteUserCommand(userName);

            //Act
            Func<Task> act = async () => await deleteUserCommandHandler.Handle(deleteUserCommand, new CancellationToken());

            //Assert
            act.Should().Throw<RestException>();
        }
    }
}