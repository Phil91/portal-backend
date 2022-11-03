/********************************************************************************
 * Copyright (c) 2021,2022 BMW Group AG
 * Copyright (c) 2021,2022 Contributors to the CatenaX (ng) GitHub Organisation.
 *
 * See the NOTICE file(s) distributed with this work for additional
 * information regarding copyright ownership.
 *
 * This program and the accompanying materials are made available under the
 * terms of the Apache License, Version 2.0 which is available at
 * https://www.apache.org/licenses/LICENSE-2.0.
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS, WITHOUT
 * WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the
 * License for the specific language governing permissions and limitations
 * under the License.
 *
 * SPDX-License-Identifier: Apache-2.0
 ********************************************************************************/

using Org.CatenaX.Ng.Portal.Backend.PortalBackend.DBAccess.Models;
using Org.CatenaX.Ng.Portal.Backend.PortalBackend.PortalEntities;
using Org.CatenaX.Ng.Portal.Backend.PortalBackend.PortalEntities.Entities;
using Org.CatenaX.Ng.Portal.Backend.PortalBackend.PortalEntities.Enums;
using Microsoft.EntityFrameworkCore;

namespace Org.CatenaX.Ng.Portal.Backend.PortalBackend.DBAccess.Repositories;

/// <inheritdoc />
public class NotificationRepository : INotificationRepository
{
    private readonly PortalDbContext _dbContext;

    /// <summary>
    ///     Creates a new instance of <see cref="NotificationRepository" />
    /// </summary>
    /// <param name="dbContext">Access to the database</param>
    public NotificationRepository(PortalDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    /// <inheritdoc />
    public Notification CreateNotification(Guid receiverUserId, NotificationTypeId notificationTypeId,
        bool isRead, Action<Notification>? setOptionalParameter = null)
    {
        var notification = new Notification(Guid.NewGuid(), receiverUserId, DateTimeOffset.UtcNow,
            notificationTypeId, isRead);
        setOptionalParameter?.Invoke(notification);

        return _dbContext.Add(notification).Entity;
    }

    public Notification AttachAndModifyNotification(Guid notificationId, Action<Notification>? setOptionalParameters = null)
    {
        var notification = _dbContext.Attach(new Notification(notificationId, Guid.Empty, default, default, default)).Entity;
        setOptionalParameters?.Invoke(notification);
        return notification;
    }

    public Notification DeleteNotification(Guid notificationId) =>
        _dbContext.Remove(new Notification(notificationId, Guid.Empty, default, default, default)).Entity;

    /// <inheritdoc />
    public IQueryable<NotificationDetailData> GetAllNotificationDetailsByIamUserIdUntracked(string iamUserId, bool? isRead, NotificationTypeId? typeId) =>
        _dbContext.Notifications
            .AsNoTracking()
            .Where(notification =>
                (notification.Receiver!.IamUser!.UserEntityId == iamUserId)
                && (!isRead.HasValue || notification.IsRead == isRead.Value)
                && (!typeId.HasValue || notification.NotificationTypeId == typeId.Value))
            .Select(notification => new NotificationDetailData(
                notification.Id,
                notification.DateCreated,
                notification.NotificationTypeId,
                notification.IsRead,
                notification.Content,
                notification.DueDate))
            .AsQueryable();

    /// <inheritdoc />
    public Task<(bool IsUserReceiver, NotificationDetailData NotificationDetailData)> GetNotificationByIdAndIamUserIdUntrackedAsync(Guid notificationId, string iamUserId) =>
        _dbContext.Notifications
            .AsNoTracking()
            .Where(notification => notification.Id == notificationId)
            .Select(notification => new ValueTuple<bool, NotificationDetailData>(
                notification.Receiver!.IamUser!.UserEntityId == iamUserId,
                new NotificationDetailData(
                    notification.Id,
                    notification.DateCreated,
                    notification.NotificationTypeId,
                    notification.IsRead,
                    notification.Content,
                    notification.DueDate)))
            .SingleOrDefaultAsync();

    /// <inheritdoc />
    public Task<(bool IsUserExisting, int Count)> GetNotificationCountForIamUserAsync(string iamUserId, bool? isRead) =>
        _dbContext.CompanyUsers
            .AsNoTracking()
            .Where(companyUser => companyUser.IamUser!.UserEntityId == iamUserId)
            .Select(companyUser => new ValueTuple<bool, int>(
                true,
                companyUser.Notifications
                    .Count(notification => !isRead.HasValue || notification.IsRead == isRead.Value)))
            .SingleOrDefaultAsync();

    /// <inheritdoc /> TODO (PS): rework count
    public Task<NotificationCountDetails?> GetCountDetailsForUserAsync(string iamUserId) =>
        _dbContext.Notifications
            .AsNoTracking()
            .Where(not => not.Receiver!.IamUser!.UserEntityId == iamUserId)
            .Select(x => new NotificationCountDetails(
                    1,
                    1,
                    1,
                    1,
                    1
                ))
            .SingleOrDefaultAsync();

    /// <inheritdoc />
    public Task<(bool IsUserReceiver, bool IsNotificationExisting)> CheckNotificationExistsByIdAndIamUserIdAsync(Guid notificationId, string iamUserId) =>
        _dbContext.Notifications
            .AsNoTracking()
            .Where(notification => notification.Id == notificationId)
            .Select(notification => new ValueTuple<bool, bool>(
                notification.Receiver!.IamUser!.UserEntityId == iamUserId,
                true))
            .SingleOrDefaultAsync();
}
