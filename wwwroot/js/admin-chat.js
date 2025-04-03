/**
 * AdminChat Module - Quản lý chức năng chat trong admin panel
 * Sử dụng Revealing Module Pattern để tổ chức code
 */
const AdminChat = (function () {
  "use strict";

  // Private variables
  let _config = {
    selectors: {
      usersList: "#users-list",
      chatMessages: "#chat-messages",
      chatInput: "#chat-input",
      messageForm: "#message-form",
      sendButton: "#send-button",
      fileInput: "#file-input",
      filePreviewContainer: "#file-preview-container",
      searchUsers: "#search-users",
      totalUsersCount: "#total-users-count",
      selectedUserName: "#selected-user-name",
      chatArea: "#chat-area",
      noChatSelected: "#no-chat-selected",
    },
    api: {
      getUsersWithChatHistory: "/Admin/AdminChat/GetUsersWithChatHistory",
      getAllUsersDetails: "/Admin/AdminChat/GetAllUsersDetails",
      getAllUnreadCounts: "/Admin/AdminChat/GetAllUnreadCounts",
      getChatHistory: "/Admin/AdminChat/GetChatHistory",
      sendMessage: "/Admin/AdminChat/SendMessage",
      markAsRead: "/Admin/AdminChat/MarkAsRead",
      updateMessage: "/Admin/api/chat/messages/",
      deleteMessage: "/Admin/api/chat/messages/",
      testConnection: "/Admin/AdminChat/TestConnection",
    },
    timeouts: {
      loadingTimeout: 30000,
      apiTimeout: {
        default: 15000,
        users: 40000,
        details: 20000,
      },
      cacheExpiry: 60000, // 1 minute
    },
  };

  // State management
  const _state = {
    adminId: null,
    selectedUserId: null,
    selectedUserName: null,
    selectedUserEmail: null,
    lastMessageTimestamp: null,
    allUsers: [],
    chatHistoryRetryCount: 0,
    MAX_RETRIES: 3,
    connection: null,
    cache: {
      usersWithChat: null,
      lastLoadTime: null,
      lastTestTime: null,
    },
  };

  /**
   * Utility Module
   */
  const Utils = (function () {
    // Format file size in a human-readable way
    function formatFileSize(bytes) {
      if (bytes < 1024) {
        return bytes + " B";
      } else if (bytes < 1024 * 1024) {
        return (bytes / 1024).toFixed(1) + " KB";
      } else {
        return (bytes / (1024 * 1024)).toFixed(2) + " MB";
      }
    }

    // Format date for message groups
    function formatMessageDate(date) {
      const today = new Date();
      const yesterday = new Date(today);
      yesterday.setDate(today.getDate() - 1);

      if (date.toDateString() === today.toDateString()) {
        return "Hôm nay";
      } else if (date.toDateString() === yesterday.toDateString()) {
        return "Hôm qua";
      } else {
        return date.toLocaleDateString("vi-VN", {
          day: "2-digit",
          month: "2-digit",
          year: "numeric",
        });
      }
    }

    // Scroll to bottom of chat window
    function scrollToBottom() {
      const chatMessages = document.querySelector(
        _config.selectors.chatMessages
      );
      if (chatMessages) {
        chatMessages.scrollTop = chatMessages.scrollHeight;
      }
    }

    // Check if message can be edited (within 1 hour)
    function canEditMessage(messageTime) {
      const now = new Date();
      const hoursDiff = (now - messageTime) / (1000 * 60 * 60);
      return hoursDiff <= 1;
    }

    // Check if message can be deleted
    function canDeleteMessage() {
      return true; // Admin can always delete their messages
    }

    // Generate HTML for message action buttons
    function getMessageActionsHTML(messageId) {
      return `
        <div class="message-actions">
          <button class="btn btn-sm edit-btn" title="Sửa" onclick="AdminChat.editMessage(${messageId})">
            <i class="fas fa-pencil-alt"></i>
          </button>
          <button class="btn btn-sm delete-btn" title="Xóa" onclick="AdminChat.deleteMessage(${messageId})">
            <i class="fas fa-trash"></i>
          </button>
        </div>
      `;
    }

    // Log with time
    function log(level, message, ...args) {
      const timestamp = new Date().toISOString().substring(11, 23);
      const prefix = `[${timestamp}][AdminChat]`;

      switch (level) {
        case "error":
          console.error(prefix, message, ...args);
          break;
        case "warn":
          console.warn(prefix, message, ...args);
          break;
        case "info":
          console.info(prefix, message, ...args);
          break;
        default:
          console.log(prefix, message, ...args);
      }
    }

    return {
      formatFileSize,
      formatMessageDate,
      scrollToBottom,
      canEditMessage,
      canDeleteMessage,
      getMessageActionsHTML,
      log,
    };
  })();

  /**
   * API Handler Module
   */
  const ApiService = (function () {
    // Generic AJAX request handler with logging and error handling
    function _request(options) {
      const defaultOptions = {
        cache: false,
        headers: {
          "Cache-Control": "no-cache",
          Pragma: "no-cache",
        },
      };

      const requestOptions = { ...defaultOptions, ...options };
      Utils.log("info", `API Request to ${requestOptions.url}`);

      return $.ajax(requestOptions)
        .done(function (response) {
          Utils.log("info", `API Success: ${requestOptions.url}`);
          return response;
        })
        .fail(function (xhr, status, error) {
          Utils.log(
            "error",
            `API Error (${requestOptions.url}):`,
            status,
            error,
            xhr
          );
          return Promise.reject({ xhr, status, error });
        });
    }

    function getUsersWithChatHistory() {
      return _request({
        url: _config.api.getUsersWithChatHistory,
        type: "GET",
        timeout: _config.timeouts.apiTimeout.users,
      });
    }

    function getAllUsersDetails(userIds) {
      return _request({
        url: _config.api.getAllUsersDetails,
        type: "POST",
        contentType: "application/json",
        data: JSON.stringify(userIds),
        timeout: _config.timeouts.apiTimeout.details,
      });
    }

    function getAllUnreadCounts(userIds) {
      return _request({
        url: _config.api.getAllUnreadCounts,
        type: "POST",
        contentType: "application/json",
        data: JSON.stringify(userIds),
        timeout: _config.timeouts.apiTimeout.default,
      });
    }

    function getChatHistory(adminId, otherUserId) {
      return _request({
        url: `${_config.api.getChatHistory}?userId=${adminId}&otherUserId=${otherUserId}`,
        type: "GET",
        timeout: _config.timeouts.apiTimeout.default,
      });
    }

    function sendMessage(formData) {
      return _request({
        url: _config.api.sendMessage,
        type: "POST",
        data: formData,
        processData: false,
        contentType: false,
      });
    }

    function markAsRead(senderId, receiverId) {
      return _request({
        url: _config.api.markAsRead,
        type: "POST",
        contentType: "application/json",
        data: JSON.stringify({
          senderId: senderId,
          receiverId: receiverId,
        }),
      });
    }

    function updateMessage(messageId, content) {
      return _request({
        url: `${_config.api.updateMessage}${messageId}`,
        type: "PUT",
        contentType: "application/json",
        data: JSON.stringify({
          content: content,
        }),
      });
    }

    function deleteMessage(messageId) {
      return _request({
        url: `${_config.api.deleteMessage}${messageId}`,
        type: "DELETE",
      });
    }

    function testConnection() {
      return _request({
        url: _config.api.testConnection,
        type: "GET",
        timeout: _config.timeouts.apiTimeout.default,
      });
    }

    return {
      getUsersWithChatHistory,
      getAllUsersDetails,
      getAllUnreadCounts,
      getChatHistory,
      sendMessage,
      markAsRead,
      updateMessage,
      deleteMessage,
      testConnection,
    };
  })();

  /**
   * SignalR Module
   */
  const SignalRService = (function () {
    async function initialize() {
      try {
        _state.connection = new signalR.HubConnectionBuilder()
          .withUrl("/chatHub")
          .withAutomaticReconnect()
          .build();

        _setupEventHandlers();

        await _state.connection.start();
        Utils.log("info", "Connected to SignalR hub");

        // Join admin group
        await _state.connection.invoke("JoinGroup", _state.adminId);
      } catch (error) {
        Utils.log("error", "Error connecting to SignalR hub:", error);
        toastr.error(
          "Không thể kết nối đến máy chủ chat. Vui lòng tải lại trang."
        );
      }
    }

    function _setupEventHandlers() {
      _state.connection.on("ReceiveMessage", UI.handleIncomingMessage);
      _state.connection.on("MessageUpdated", UI.handleMessageUpdated);
      _state.connection.on("MessageDeleted", UI.handleMessageDeleted);
      _state.connection.on("UpdateUnreadCount", UI.handleUnreadCountUpdated);
    }

    return {
      initialize,
    };
  })();

  /**
   * UI Module - Handles all UI-related functions
   */
  const UI = (function () {
    function renderUserList(users) {
      Utils.log(
        "info",
        `Rendering ${users ? users.length : 0} users in the list`
      );
      const usersList = $(_config.selectors.usersList);
      usersList.empty();

      if (!users || users.length === 0) {
        _renderEmptyUserList(usersList);
        return;
      }

      // Sort users by unread count (descending) and then by name (ascending)
      const sortedUsers = _sortUsers(users);
      const unreadUsers = sortedUsers.filter((user) => user.unreadCount > 0);
      const readUsers = sortedUsers.filter((user) => user.unreadCount === 0);

      // Add section headers and render users
      if (unreadUsers.length > 0) {
        _renderUserSection(usersList, "Tin nhắn chưa đọc", unreadUsers);
      }

      if (readUsers.length > 0) {
        _renderUserSection(
          usersList,
          "Tin nhắn đã đọc",
          readUsers,
          unreadUsers.length > 0
        );
      }

      // Add debug button at the bottom
      _addDebugButton(usersList);
      _ensureSectionStyles();
    }

    function _renderEmptyUserList(container) {
      container.append(`
        <div class="text-center text-muted p-4">
          <i class="fas fa-comments fa-3x mb-3 text-muted"></i>
          <p>Không có người dùng nào có lịch sử trò chuyện</p>
          <div class="mt-2">
            <button class="btn btn-sm btn-outline-primary" onclick="AdminChat.loadUsersWithChatHistory(true)">
              <i class="fas fa-sync-alt"></i> Làm mới
            </button>
            <button class="btn btn-sm btn-outline-secondary ms-2" onclick="AdminChat.showDiagnostics()">
              <i class="fas fa-bug"></i> Xem lỗi
            </button>
          </div>
        </div>
      `);
    }

    function _sortUsers(users) {
      return users.sort((a, b) => {
        // First sort by unread count (descending)
        if (b.unreadCount !== a.unreadCount) {
          return b.unreadCount - a.unreadCount;
        }
        // Then sort by name (ascending)
        if (a.name && b.name) {
          return a.name.localeCompare(b.name);
        }
        return 0;
      });
    }

    function _renderUserSection(container, title, users, withMargin = false) {
      container.append(`
        <div class="chat-section-header ${withMargin ? "mt-3" : ""}">
          <span>${title} (${users.length})</span>
        </div>
      `);

      users.forEach((user) => _renderUserItem(container, user));
    }

    function _renderUserItem(container, user) {
      const hasUnread = user.unreadCount > 0;
      const avatarClass = hasUnread
        ? "chat-user-avatar has-unread"
        : "chat-user-avatar";

      const userItem = $(`
        <div class="chat-user" data-user-id="${user.id}">
          <div class="${avatarClass}">
            <img src="${
              user.avatar || "/images/web-tuyen-dung-dashboard-1.png"
            }" alt="${user.name}" 
                 onerror="this.src='/images/web-tuyen-dung-dashboard-1.png'">
          </div>
          <div class="chat-user-info">
            <div class="chat-user-name">${user.name}</div>
            <div class="chat-user-status">${
              user.lastActive || "Không hoạt động"
            }</div>
          </div>
          ${
            hasUnread
              ? `<div class="chat-user-unread">${user.unreadCount}</div>`
              : ""
          }
        </div>
      `);

      userItem.on("click", function () {
        ChatManager.loadChatHistory(user.id, user.name);
        // Mark as highlighted/active
        $(".chat-user").removeClass("active");
        $(this).addClass("active");

        // Update UI
        $(_config.selectors.selectedUserName).text(user.name);
        $(_config.selectors.chatArea).show();
        $(_config.selectors.noChatSelected).hide();

        // Update unread count after clicking
        if (hasUnread) {
          $(this).find(".chat-user-unread").remove();
          // Also update the user object so the count is correct if we re-render
          user.unreadCount = 0;
        }
      });

      container.append(userItem);
    }

    function _addDebugButton(container) {
      container.append(`
        <div class="text-center mt-3">
          <button class="btn btn-sm btn-outline-secondary" onclick="AdminChat.showDiagnostics()">
            <i class="fas fa-bug"></i> Chẩn đoán
          </button>
        </div>
      `);
    }

    function _ensureSectionStyles() {
      if ($("style#chat-section-styles").length === 0) {
        $("head").append(`
          <style id="chat-section-styles">
            .chat-section-header {
              padding: 5px 15px;
              background-color: #f8f9fa;
              border-radius: 5px;
              font-size: 14px;
              font-weight: 500;
              color: #495057;
              margin-bottom: 10px;
            }
          </style>
        `);
      }
    }

    function showLoadingUsers() {
      $(_config.selectors.usersList).empty().append(`
        <div class="text-center p-4">
          <div class="spinner-border text-primary" role="status">
            <span class="visually-hidden">Đang tải...</span>
          </div>
          <p class="mt-2">Đang tải danh sách người dùng...</p>
        </div>
      `);
    }

    function showLoadingTimeout() {
      $(_config.selectors.usersList).empty().append(`
        <div class="text-center text-danger p-4">
          <p>Quá trình tải đang mất nhiều thời gian hơn dự kiến.</p>
          <p>Vui lòng đợi hoặc làm mới trang.</p>
          <button class="btn btn-primary mt-2" onclick="AdminChat.loadUsersWithChatHistory(true)">
            <i class="fas fa-sync-alt"></i> Thử lại
          </button>
        </div>
      `);
    }

    function showLoadingUserDetails(progress = 30) {
      $(_config.selectors.usersList).empty().append(`
        <div class="text-center p-4">
          <div class="progress mb-3" style="height: 20px;">
            <div class="progress-bar progress-bar-striped progress-bar-animated" 
                 role="progressbar" aria-valuenow="${progress}" aria-valuemin="0" 
                 aria-valuemax="100" style="width: ${progress}%">${progress}%</div>
          </div>
          <p>Đang tải thông tin người dùng (1/2)...</p>
        </div>
      `);
    }

    function updateLoadingProgress(progress, message) {
      $(_config.selectors.usersList + " .progress-bar")
        .css("width", `${progress}%`)
        .text(`${progress}%`);
      $(_config.selectors.usersList + " p").text(message);
    }

    function showLoadingError(status, error) {
      $(_config.selectors.usersList).empty().append(`
        <div class="text-center text-danger p-4">
          <p>Không thể tải danh sách người dùng</p>
          <p>Lỗi: ${status} - ${error || "Unknown error"}</p>
          <p class="small text-muted">Status code: ${
            status === 0 ? "Network error" : status
          }</p>
          <button class="btn btn-primary mt-2" onclick="AdminChat.loadUsersWithChatHistory(true)">
            <i class="fas fa-sync-alt"></i> Thử lại
          </button>
        </div>
      `);

      // Add specific message for network errors
      if (status === 0) {
        $(_config.selectors.usersList).append(`
          <div class="alert alert-warning mt-3">
            <p>Có vẻ như đây là lỗi mạng hoặc CORS. Vui lòng kiểm tra kết nối.</p>
          </div>
        `);
      }
    }

    function showLoadingTime(time) {
      $(_config.selectors.usersList).append(`
        <div class="text-muted small text-center mt-3">
          Tải xong trong ${(time / 1000).toFixed(1)} giây
        </div>
      `);
    }

    function updateTotalUsersCount(count) {
      $(_config.selectors.totalUsersCount).text(count);
    }

    function showEmptyChatHistory() {
      $(_config.selectors.chatMessages).html(`
        <div class="text-center empty-chat-state">
          <div class="empty-chat-icon">
            <i class="fas fa-comments fa-3x mb-3 text-primary opacity-5"></i>
          </div>
          <h6 class="text-muted">Chưa có tin nhắn nào</h6>
          <p class="text-sm opacity-8">Bắt đầu cuộc trò chuyện ngay bây giờ!</p>
        </div>
      `);
    }

    function showLoadingChatHistory(isRetry = false, retryCount = 0) {
      $(_config.selectors.chatMessages).html(`
        <div class="text-center p-4">
          <div class="spinner-border text-primary" role="status">
            <span class="visually-hidden">Đang tải...</span>
          </div>
          <p class="mt-2">Đang tải tin nhắn...${
            isRetry ? ` (Thử lại lần ${retryCount})` : ""
          }</p>
        </div>
      `);
    }

    function showChatHistoryError(error, adminId, otherUserId) {
      $(_config.selectors.chatMessages).html(`
        <div class="alert alert-danger m-3">
          <h6><i class="fas fa-exclamation-triangle me-2"></i>Lỗi</h6>
          <p>${error}</p>
          <div class="mt-3">
            <button class="btn btn-sm btn-outline-danger" onclick="AdminChat.loadChatHistory('${adminId}', '${otherUserId}')">
              <i class="fas fa-sync-alt me-1"></i>Thử lại
            </button>
          </div>
        </div>
      `);
    }

    function appendMessage(message) {
      const isAdmin = message.senderId === _state.adminId;
      const canEdit =
        isAdmin && Utils.canEditMessage(new Date(message.timestamp));
      const canDelete =
        isAdmin && Utils.canDeleteMessage(new Date(message.timestamp));
      const messageTime = new Date(message.timestamp).toLocaleTimeString([], {
        hour: "2-digit",
        minute: "2-digit",
      });

      const formattedDate = Utils.formatMessageDate(
        new Date(message.timestamp)
      );

      // Add date separator if needed
      _addDateSeparatorIfNeeded(formattedDate);

      // Parse message content for file attachments
      const { messageContent, fileAttachment } = _parseMessageContent(
        message.content
      );

      // Safely check if message is edited - handle case where isEdited might not exist
      const isEdited = message.isEdited === true;

      const messageHtml = `
        <div class="chat-message ${
          isAdmin ? "admin" : "user"
        }" data-message-id="${message.id}">
          <div class="message-bubble">
            ${messageContent}
            ${fileAttachment}
            ${isAdmin ? Utils.getMessageActionsHTML(message.id) : ""}
          </div>
          <div class="message-time-wrapper">
            <span class="message-time">${messageTime}${
        isEdited ? " (đã chỉnh sửa)" : ""
      }</span>
          </div>
        </div>
      `;

      $(_config.selectors.chatMessages).append(messageHtml);
      Utils.scrollToBottom();
    }

    function _addDateSeparatorIfNeeded(formattedDate) {
      const lastDateEl = $(".date-separator").last();
      const lastDate = lastDateEl.length ? lastDateEl.data("date") : null;

      if (!lastDate || lastDate !== formattedDate) {
        $(_config.selectors.chatMessages).append(`
          <div class="text-center my-3">
            <span class="date-separator badge bg-light text-dark" data-date="${formattedDate}">
              ${formattedDate}
            </span>
          </div>
        `);
      }
    }

    function _parseMessageContent(content) {
      let messageContent = content;
      let fileAttachment = "";

      if (content.includes("[FILE]")) {
        const contentParts = content.split("\n[FILE]");
        messageContent = contentParts[0];

        // Extract file information
        const fileInfo = contentParts[1].split("\n");
        const fileUrl = fileInfo[0];
        const fileName = fileInfo[1];
        const fileExt = fileInfo[2];

        // Check if it's an image
        const isImage = [".jpg", ".jpeg", ".png", ".gif"].includes(
          fileExt.toLowerCase()
        );

        if (isImage) {
          // Display image
          fileAttachment = `
            <div class="attachment-image">
              <a href="${fileUrl}" target="_blank">
                <img src="${fileUrl}" alt="${fileName}" class="img-fluid rounded">
              </a>
            </div>
          `;
        } else {
          // Display file link with icon
          let iconClass = "fas fa-file";
          if (fileExt.includes(".pdf")) {
            iconClass = "fas fa-file-pdf";
          } else if (fileExt.includes(".doc")) {
            iconClass = "fas fa-file-word";
          } else if (fileExt.includes(".xls")) {
            iconClass = "fas fa-file-excel";
          } else if (fileExt.includes(".txt")) {
            iconClass = "fas fa-file-alt";
          }

          fileAttachment = `
            <div class="attachment-file">
              <a href="${fileUrl}" target="_blank" class="file-link">
                <div class="file-icon">
                  <i class="${iconClass}"></i>
                </div>
                <div class="file-info">
                  <div class="file-name">${fileName}</div>
                </div>
              </a>
            </div>
          `;
        }
      }

      return { messageContent, fileAttachment };
    }

    function handleIncomingMessage(message) {
      if (
        _state.selectedUserId === message.senderId ||
        _state.selectedUserId === message.receiverId
      ) {
        appendMessage(message);

        // If message is from the selected user, mark as read
        if (message.senderId === _state.selectedUserId) {
          ChatManager.markAsRead(_state.selectedUserId);
        }
      } else {
        // If message is from another user, update their unread count
        incrementUnreadCount(message.senderId);
      }
    }

    function handleMessageUpdated(message) {
      const messageElement = document.querySelector(
        `[data-message-id="${message.id}"]`
      );
      if (messageElement) {
        messageElement.querySelector(".message-bubble").textContent =
          message.content;
        messageElement.querySelector(".message-time").textContent +=
          " (đã chỉnh sửa)";
      }
    }

    function handleMessageDeleted(messageId) {
      const messageElement = document.querySelector(
        `[data-message-id="${messageId}"]`
      );
      if (messageElement) {
        messageElement.classList.add("fade-out");
        setTimeout(() => {
          messageElement.remove();
        }, 300);
      }
    }

    function handleUnreadCountUpdated(count) {
      // Update UI with unread count notification
      // For example, update a badge on navigation
    }

    function incrementUnreadCount(userId) {
      const badge = $(`#unread-${userId}`);
      let count = parseInt(badge.text()) || 0;
      badge.text(count + 1).show();

      // If user not in list, reload list
      if (badge.length === 0) {
        ChatManager.loadUsersWithChatHistory();
      }
    }

    function showEditMessage(messageId, messageContent) {
      const messageBubble = $(
        `.chat-message[data-message-id="${messageId}"] .message-bubble`
      );
      messageBubble.html(`
        <div class="edit-container">
          <textarea class="form-control edit-input" rows="2">${messageContent}</textarea>
          <div class="edit-actions">
            <button class="btn btn-sm btn-light cancel-edit">Hủy</button>
            <button class="btn btn-sm btn-primary save-edit">Lưu</button>
          </div>
        </div>
      `);

      // Focus on input
      const editInput = messageBubble.find(".edit-input");
      editInput.focus();

      // Handle save and cancel
      messageBubble.find(".save-edit").on("click", function () {
        ChatManager.saveEditedMessage(messageId, editInput.val().trim());
      });

      messageBubble.find(".cancel-edit").on("click", function () {
        ChatManager.cancelEditMessage(messageId, messageContent);
      });

      // Save on Ctrl+Enter
      editInput.on("keydown", function (e) {
        if (e.key === "Enter" && e.ctrlKey) {
          ChatManager.saveEditedMessage(messageId, editInput.val().trim());
        }
        // Cancel on Escape
        if (e.key === "Escape") {
          ChatManager.cancelEditMessage(messageId, messageContent);
        }
      });
    }

    function restoreMessageContent(messageId, originalContent) {
      const messageElement = $(`.chat-message[data-message-id="${messageId}"]`);
      if (messageElement.length === 0) return;

      // Restore original content
      messageElement.find(".message-bubble").text(originalContent);

      // Restore original actions
      messageElement
        .find(".message-bubble")
        .append(Utils.getMessageActionsHTML(messageId));
    }

    function updateMessageAfterEdit(messageId, newContent, isEdited) {
      const messageElement = $(`.chat-message[data-message-id="${messageId}"]`);
      if (messageElement.length === 0) return;

      // Update message content
      messageElement.find(".message-bubble").text(newContent);

      // Update message actions
      messageElement
        .find(".message-bubble")
        .append(Utils.getMessageActionsHTML(messageId));

      // Mark message as edited if needed
      if (isEdited) {
        const timeEl = messageElement.find(".message-time");
        if (!timeEl.text().includes("(đã chỉnh sửa)")) {
          timeEl.text(timeEl.text() + " (đã chỉnh sửa)");
        }
      }
    }

    function showFilePreview(file) {
      // Clear any existing preview
      $(_config.selectors.filePreviewContainer).empty();

      // Check if file is an image
      const isImage = file.type.startsWith("image/");

      // Create preview container
      const previewContainer = $("<div>").addClass("file-preview-container");

      // Add image preview or file icon
      if (isImage) {
        const reader = new FileReader();
        reader.onload = function (e) {
          const img = $("<img>").attr("src", e.target.result);
          previewContainer.append(img);
        };
        reader.readAsDataURL(file);
      } else {
        // File icon based on extension
        let fileIcon = "fa-file";
        const extension = file.name.split(".").pop().toLowerCase();

        if (["pdf"].includes(extension)) {
          fileIcon = "fa-file-pdf";
        } else if (["doc", "docx"].includes(extension)) {
          fileIcon = "fa-file-word";
        } else if (["xls", "xlsx"].includes(extension)) {
          fileIcon = "fa-file-excel";
        } else if (["zip", "rar", "7z"].includes(extension)) {
          fileIcon = "fa-file-archive";
        } else if (["txt", "log"].includes(extension)) {
          fileIcon = "fa-file-alt";
        }

        const icon = $("<i>")
          .addClass(`fas ${fileIcon} fa-3x`)
          .css("color", "#6c757d");
        previewContainer.append(icon);
      }

      // Add file info
      const fileInfo = $("<div>").addClass("file-info");
      const fileName = $("<div>").addClass("file-name").text(file.name);
      const fileSize = $("<div>")
        .addClass("file-size")
        .text(Utils.formatFileSize(file.size));
      fileInfo.append(fileName, fileSize);
      previewContainer.append(fileInfo);

      // Add remove button
      const removeBtn = $("<div>")
        .addClass("remove-file")
        .html('<i class="fas fa-times"></i>')
        .on("click", function () {
          $("#file-input").val("");
          $(_config.selectors.filePreviewContainer).empty();
        });
      previewContainer.append(removeBtn);

      // Add to form
      $(_config.selectors.filePreviewContainer).append(previewContainer);
    }

    function showDiagnosticsModal(diagnosticsData) {
      // Create and show modal with diagnostic information
      const modal = $(`
        <div class="modal fade" id="diagnosticsModal" tabindex="-1" aria-hidden="true">
          <div class="modal-dialog">
            <div class="modal-content">
              <div class="modal-header">
                <h5 class="modal-title">Thông tin chẩn đoán</h5>
                <button type="button" class="btn-close" data-bs-dismiss="modal" aria-label="Close"></button>
              </div>
              <div class="modal-body">
                <div class="mb-3">
                  <p><strong>Trình duyệt:</strong> ${
                    diagnosticsData.browser
                  }</p>
                  <p><strong>Thời gian tải cuối:</strong> ${
                    diagnosticsData.loadTime
                  }</p>
                  <p><strong>Trạng thái cache:</strong> ${
                    diagnosticsData.cacheStatus
                  }</p>
                  <p><strong>Số người dùng:</strong> ${
                    diagnosticsData.userCount
                  }</p>
                  <p><strong>Thời gian hiện tại:</strong> ${new Date().toLocaleString()}</p>
                </div>
                <hr>
                <div class="d-grid">
                  <button class="btn btn-primary" onclick="AdminChat.testApiConnection()">
                    <i class="fas fa-network-wired"></i> Kiểm tra kết nối API
                  </button>
                </div>
                <div id="apiTestResults" class="mt-3"></div>
              </div>
              <div class="modal-footer">
                <button type="button" class="btn btn-secondary" data-bs-dismiss="modal">Đóng</button>
                <button type="button" class="btn btn-primary" onclick="AdminChat.loadUsersWithChatHistory(true); $('#diagnosticsModal').modal('hide');">
                  <i class="fas fa-sync-alt"></i> Làm mới danh sách
                </button>
              </div>
            </div>
          </div>
        </div>
      `);

      // Add to page
      $("body").append(modal);
      const bsModal = new bootstrap.Modal(
        document.getElementById("diagnosticsModal")
      );
      bsModal.show();

      // Remove modal when closed
      modal.on("hidden.bs.modal", function () {
        modal.remove();
      });
    }

    function updateApiTestResults(html) {
      $("#apiTestResults").html(html);
    }

    function showSendingState() {
      $(_config.selectors.sendButton).prop("disabled", true);
      $(_config.selectors.sendButton).html(
        '<span class="spinner-border spinner-border-sm" role="status" aria-hidden="true"></span>'
      );
    }

    function resetSendingState() {
      $(_config.selectors.sendButton).prop("disabled", false);
      $(_config.selectors.sendButton).html(
        '<i class="fas fa-paper-plane"></i>'
      );
    }

    function clearInputs() {
      $(_config.selectors.chatInput).val("");
      $(_config.selectors.filePreviewContainer).empty();
      $("#file-input").val("");
    }

    return {
      renderUserList,
      showLoadingUsers,
      showLoadingTimeout,
      showLoadingUserDetails,
      updateLoadingProgress,
      showLoadingError,
      showLoadingTime,
      updateTotalUsersCount,
      showEmptyChatHistory,
      showLoadingChatHistory,
      showChatHistoryError,
      appendMessage,
      handleIncomingMessage,
      handleMessageUpdated,
      handleMessageDeleted,
      handleUnreadCountUpdated,
      incrementUnreadCount,
      showEditMessage,
      restoreMessageContent,
      updateMessageAfterEdit,
      showFilePreview,
      showDiagnosticsModal,
      updateApiTestResults,
      showSendingState,
      resetSendingState,
      clearInputs,
    };
  })();

  /**
   * Chat Manager Module - Handles chat business logic
   */
  const ChatManager = (function () {
    /**
     * Load users with chat history
     * @param {boolean} forceRefresh - Force refresh cache
     */
    function loadUsersWithChatHistory(forceRefresh = false) {
      Utils.log(
        "info",
        `Loading users with chat history... (forceRefresh: ${forceRefresh})`
      );
      const startTime = performance.now();

      // Use cache if available and not forcing refresh
      if (
        !forceRefresh &&
        _state.cache.usersWithChat &&
        _state.cache.lastLoadTime
      ) {
        const cacheAge = Date.now() - _state.cache.lastLoadTime;
        if (cacheAge < _config.timeouts.cacheExpiry) {
          Utils.log("info", `Using cached data (${cacheAge / 1000}s old)`);
          UI.renderUserList(_state.cache.usersWithChat);
          return;
        }
      }

      // Show loading state
      UI.showLoadingUsers();

      // Set a timeout to handle slow loads
      const loadingTimeout = setTimeout(function () {
        Utils.log("warn", "Loading timeout triggered after 30 seconds");
        UI.showLoadingTimeout();
      }, _config.timeouts.loadingTimeout);

      // Get users with chat history
      ApiService.getUsersWithChatHistory()
        .then(function (users) {
          const usersTime = performance.now() - startTime;
          Utils.log(
            "info",
            `Got ${users ? users.length : 0} users in ${usersTime.toFixed(2)}ms`
          );
          clearTimeout(loadingTimeout);

          if (!users || users.length === 0) {
            Utils.log("warn", "No users returned from API");
            UI.updateTotalUsersCount(0);
            UI.renderUserList([]);
            return;
          }

          // Update total count
          UI.updateTotalUsersCount(users.length);

          // Show loading progress for user details
          UI.showLoadingUserDetails(30);

          // Get user details
          return _loadUserDetails(users, startTime).then((userDetails) => {
            // Update progress
            UI.updateLoadingProgress(
              65,
              "Đang tải số tin nhắn chưa đọc (2/2)..."
            );

            // Get unread counts
            return _loadUnreadCounts(users, userDetails, startTime);
          });
        })
        .catch(function (error) {
          Utils.log("error", "Error loading users:", error);
          clearTimeout(loadingTimeout);

          UI.showLoadingError(error.status, error.error);
        });
    }

    /**
     * Load user details
     * @param {Array} users - User IDs
     * @param {number} startTime - Start time for performance tracking
     */
    function _loadUserDetails(users, startTime) {
      return ApiService.getAllUsersDetails(users).then(function (userDetails) {
        const detailsTime = performance.now() - startTime;
        Utils.log("info", `Got user details in ${detailsTime.toFixed(2)}ms`);
        return userDetails;
      });
    }

    /**
     * Load unread message counts for users
     * @param {Array} users - User IDs
     * @param {Object} userDetails - User details
     * @param {number} startTime - Start time for performance tracking
     */
    function _loadUnreadCounts(users, userDetails, startTime) {
      return ApiService.getAllUnreadCounts(users)
        .then(function (unreadCounts) {
          const totalTime = performance.now() - startTime;
          Utils.log("info", `Total loading time: ${totalTime.toFixed(2)}ms`);

          // Combine all data
          const combinedUsers = _combineUserData(
            users,
            userDetails,
            unreadCounts
          );

          // Save to cache
          _state.cache.usersWithChat = combinedUsers;
          _state.cache.lastLoadTime = Date.now();

          // Render the list
          UI.renderUserList(combinedUsers);

          // Show total loading time
          UI.showLoadingTime(totalTime);

          return combinedUsers;
        })
        .catch(function (error) {
          Utils.log("error", "Error loading unread counts:", error);

          // Use available data without unread counts
          const combinedUsers = _combineUserData(users, userDetails, {});

          _state.cache.usersWithChat = combinedUsers;
          _state.cache.lastLoadTime = Date.now();

          UI.renderUserList(combinedUsers);
          return combinedUsers;
        });
    }

    /**
     * Combine user data from different sources
     * @param {Array} userIds - User IDs
     * @param {Object} userDetails - User details
     * @param {Object} unreadCounts - Unread message counts
     */
    function _combineUserData(userIds, userDetails, unreadCounts) {
      return userIds.map((userId) => {
        const details = userDetails[userId] || {
          name: `User ${userId.substring(0, 6)}...`,
          email: "",
        };

        return {
          id: userId,
          name:
            details.name ||
            details.userName ||
            `User ${userId.substring(0, 6)}...`,
          email: details.email || "",
          avatar: details.avatar || "",
          unreadCount: unreadCounts[userId] || 0,
        };
      });
    }

    /**
     * Load chat history for a selected user
     * @param {string} userId - User ID
     * @param {string} userName - User name
     */
    function loadChatHistory(userId, userName) {
      // Set selected user
      _state.selectedUserId = userId;
      _state.selectedUserName = userName;

      // Load chat history with selected user
      _loadChatMessagesHistory(_state.adminId, userId);

      // Mark messages as read
      markAsRead(userId);

      // Enable chat input and buttons
      $(_config.selectors.chatInput).prop("disabled", false);
      $(_config.selectors.sendButton).prop("disabled", false);
      $("#emoji-button").prop("disabled", false);
      $("#attachment-button").prop("disabled", false);

      // Update UI to show selected chat
      $(_config.selectors.selectedUserName).text(userName);
      $("#no-chat-selected").addClass("d-none").removeClass("d-flex");
      $(_config.selectors.chatArea).removeClass("d-none").addClass("d-flex");
    }

    /**
     * Load chat messages history
     * @param {string} adminId - Admin ID
     * @param {string} otherUserId - Other user ID
     * @param {boolean} isRetry - Whether this is a retry attempt
     */
    function _loadChatMessagesHistory(adminId, otherUserId, isRetry = false) {
      // Only reset retry count for fresh loads
      if (!isRetry) {
        _state.chatHistoryRetryCount = 0;
      }

      // Show loading state
      UI.showLoadingChatHistory(isRetry, _state.chatHistoryRetryCount);

      ApiService.getChatHistory(adminId, otherUserId)
        .then(function (messages) {
          $(_config.selectors.chatMessages).empty();
          // Reset retry count on success
          _state.chatHistoryRetryCount = 0;

          if (!messages || messages.length === 0) {
            UI.showEmptyChatHistory();
            return;
          }

          // Display messages
          messages.forEach(function (message) {
            UI.appendMessage(message);
          });

          // Scroll to bottom
          Utils.scrollToBottom();
        })
        .catch(function (error) {
          Utils.log("error", "Error loading chat history:", error);

          // Try to auto-retry if network error or timeout and under max retries
          if (
            (error.status === "timeout" ||
              error.xhr.status === 0 ||
              error.xhr.status >= 500) &&
            _state.chatHistoryRetryCount < _state.MAX_RETRIES
          ) {
            _state.chatHistoryRetryCount++;
            Utils.log(
              "info",
              `Auto-retrying (${_state.chatHistoryRetryCount}/${_state.MAX_RETRIES})...`
            );

            // Wait a bit before retrying (exponential backoff)
            const retryDelay = Math.min(
              1000 * Math.pow(2, _state.chatHistoryRetryCount - 1),
              5000
            );
            setTimeout(() => {
              _loadChatMessagesHistory(adminId, otherUserId, true);
            }, retryDelay);
            return;
          }

          // Parse error message
          let errorMessage = _parseErrorMessage(error);
          UI.showChatHistoryError(errorMessage, adminId, otherUserId);
          toastr.error(errorMessage);
        });
    }

    /**
     * Parse error message from AJAX response
     * @param {Object} error - Error object
     */
    function _parseErrorMessage(error) {
      let errorMessage = "Không thể tải lịch sử trò chuyện";
      try {
        if (error.xhr.responseJSON && error.xhr.responseJSON.message) {
          errorMessage = error.xhr.responseJSON.message;
        } else if (error.xhr.responseText) {
          try {
            const responseData = JSON.parse(error.xhr.responseText);
            errorMessage = responseData.message || errorMessage;
          } catch (e) {
            // If not JSON, use raw text if it's short
            if (error.xhr.responseText.length < 100) {
              errorMessage = error.xhr.responseText;
            }
          }
        }
      } catch (e) {
        Utils.log("error", "Error parsing response:", e);
      }
      return errorMessage;
    }

    /**
     * Mark messages as read
     * @param {string} senderId - Sender ID
     */
    function markAsRead(senderId) {
      if (!senderId || !_state.adminId) return;

      ApiService.markAsRead(senderId, _state.adminId)
        .then(function () {
          $(`#unread-${senderId}`).text("").hide();
        })
        .catch(function (error) {
          Utils.log("error", "Error marking messages as read:", error);

          let errorMessage = "Không thể đánh dấu tin nhắn là đã đọc";
          try {
            if (error.xhr.responseJSON && error.xhr.responseJSON.message) {
              errorMessage = error.xhr.responseJSON.message;
            }
          } catch (e) {
            Utils.log("error", "Failed to parse error response:", e);
          }

          toastr.error(errorMessage);
        });
    }

    /**
     * Send message to selected user
     * @param {FormData} formData - Form data containing message and attachments
     */
    function sendMessage(formData) {
      UI.showSendingState();

      ApiService.sendMessage(formData)
        .then(function () {
          UI.clearInputs();
          UI.resetSendingState();
        })
        .catch(function (error) {
          Utils.log("error", "Error sending message:", error);

          let errorMessage = "Không thể gửi tin nhắn";
          try {
            if (error.xhr.responseJSON && error.xhr.responseJSON.message) {
              errorMessage = error.xhr.responseJSON.message;
            }
          } catch (e) {}

          toastr.error(errorMessage);
          UI.resetSendingState();
        });
    }

    /**
     * Show edit message interface
     * @param {number} messageId - Message ID
     */
    function editMessage(messageId) {
      const messageElement = document.querySelector(
        `[data-message-id="${messageId}"]`
      );
      if (!messageElement) return;

      const messageBubble = messageElement.querySelector(".message-bubble");
      const messageContent = messageBubble.textContent.trim();

      UI.showEditMessage(messageId, messageContent);
    }

    /**
     * Save edited message
     * @param {number} messageId - Message ID
     * @param {string} newContent - New message content
     */
    function saveEditedMessage(messageId, newContent) {
      if (!newContent) {
        toastr.error("Tin nhắn không được để trống");
        return;
      }

      const messageElement = document.querySelector(
        `[data-message-id="${messageId}"]`
      );
      const originalContent = messageElement
        .querySelector(".message-bubble")
        .textContent.trim();

      // Update UI immediately for better UX
      UI.updateMessageAfterEdit(messageId, newContent, true);

      // Save to server
      ApiService.updateMessage(messageId, newContent)
        .then(function () {
          toastr.success("Tin nhắn đã được cập nhật thành công");
        })
        .catch(function (error) {
          Utils.log("error", "Error updating message:", error);

          // Restore original content on error
          UI.updateMessageAfterEdit(messageId, originalContent, false);

          let errorMessage = "Không thể cập nhật tin nhắn";
          try {
            if (error.xhr.responseJSON && error.xhr.responseJSON.message) {
              errorMessage = error.xhr.responseJSON.message;
            }
          } catch (e) {}

          toastr.error(errorMessage);
        });
    }

    /**
     * Cancel message editing
     * @param {number} messageId - Message ID
     * @param {string} originalContent - Original message content
     */
    function cancelEditMessage(messageId, originalContent) {
      UI.restoreMessageContent(messageId, originalContent);
    }

    /**
     * Delete message
     * @param {number} messageId - Message ID
     */
    function deleteMessage(messageId) {
      if (!confirm("Bạn có chắc chắn muốn xóa tin nhắn này?")) {
        return;
      }

      ApiService.deleteMessage(messageId)
        .then(function () {
          // UI will be updated via SignalR
          toastr.success("Tin nhắn đã được xóa thành công");
        })
        .catch(function (error) {
          Utils.log("error", "Error deleting message:", error);

          let errorMessage = "Không thể xóa tin nhắn";
          try {
            if (error.xhr.responseJSON && error.xhr.responseJSON.message) {
              errorMessage = error.xhr.responseJSON.message;
            }
          } catch (e) {}

          toastr.error(errorMessage);
        });
    }

    /**
     * Show diagnostics dialog with system information
     */
    function showDiagnostics() {
      Utils.log("info", "Showing user load diagnostics");

      const diagnosticsData = {
        browser: navigator.userAgent,
        loadTime: _state.cache.lastLoadTime
          ? new Date(_state.cache.lastLoadTime).toLocaleString()
          : "Not loaded yet",
        cacheStatus: _state.cache.usersWithChat ? "Available" : "Empty",
        userCount: _state.cache.usersWithChat
          ? _state.cache.usersWithChat.length
          : 0,
        timestamp: new Date().toISOString(),
      };

      UI.showDiagnosticsModal(diagnosticsData);
    }

    /**
     * Test API connection and display results
     */
    function testApiConnection() {
      UI.updateApiTestResults(`
        <div class="alert alert-info">
          <div class="spinner-border spinner-border-sm" role="status"></div>
          Đang kiểm tra kết nối...
        </div>
      `);

      // Test connection API
      ApiService.testConnection()
        .then(function (data) {
          // Display results from test endpoint
          UI.updateApiTestResults(`
            <div class="alert alert-success">
              <p><i class="fas fa-check-circle"></i> Kết nối thành công!</p>
              <p><strong>Thời gian server:</strong> ${data.timestamp}</p>
              <p><strong>Admin ID:</strong> ${data.adminId}</p>
              <p><strong>Môi trường:</strong> ${data.environment}</p>
              <p><strong>Server:</strong> ${data.server}</p>
              <hr>
              <p class="mb-0">Kiểm tra danh sách người dùng...</p>
            </div>
          `);

          // After successful TestConnection, test GetUsersWithChatHistory
          setTimeout(function () {
            _testUsersApi();
          }, 1000);
        })
        .catch(function (error) {
          UI.updateApiTestResults(`
            <div class="alert alert-danger">
              <p><i class="fas fa-exclamation-circle"></i> Lỗi kết nối đến API!</p>
              <p>Status: ${error.status}, Error: ${error.error || "Unknown"}</p>
              <p>Status code: ${error.xhr ? error.xhr.status : "N/A"}</p>
              ${
                error.xhr && error.xhr.responseText
                  ? `<p>Response: ${error.xhr.responseText}</p>`
                  : ""
              }
            </div>
          `);
        });
    }

    /**
     * Test users API as part of connection test
     */
    function _testUsersApi() {
      ApiService.getUsersWithChatHistory()
        .then(function (users) {
          $("#apiTestResults").append(`
            <div class="alert alert-success mt-2">
              <p><i class="fas fa-check-circle"></i> Lấy danh sách người dùng thành công!</p>
              <p>Đã nhận được ${users ? users.length : 0} người dùng.</p>
              <p>Thời gian: ${new Date().toLocaleTimeString()}</p>
              ${
                users && users.length > 0
                  ? `<p>IDs: ${users.slice(0, 3).join(", ")}${
                      users.length > 3 ? "..." : ""
                    }</p>`
                  : ""
              }
            </div>
          `);

          // Update cache after successful test
          if (users && users.length > 0) {
            _state.cache.lastTestTime = Date.now();
            // Refresh user list
            setTimeout(() => {
              $("#apiTestResults").append(`
                <div class="alert alert-info mt-2">
                  <p>Tự động làm mới danh sách người dùng...</p>
                </div>
              `);
              loadUsersWithChatHistory(true);
            }, 1000);
          }
        })
        .catch(function (error) {
          $("#apiTestResults").append(`
            <div class="alert alert-danger mt-2">
              <p><i class="fas fa-exclamation-circle"></i> Lỗi lấy danh sách người dùng!</p>
              <p>Status: ${error.status}, Error: ${error.error || "Unknown"}</p>
              <p>Status code: ${error.xhr ? error.xhr.status : "N/A"}</p>
              ${
                error.xhr && error.xhr.responseText
                  ? `<p>Response: ${error.xhr.responseText}</p>`
                  : ""
              }
            </div>
          `);
        });
    }

    /**
     * Handle file selection for upload
     * @param {Event} event - File input change event
     */
    function handleFileSelection(event) {
      const file = event.target.files[0];
      if (!file) return;

      // Check file size (5MB max)
      const maxSize = 5 * 1024 * 1024; // 5MB
      if (file.size > maxSize) {
        toastr.error("Kích thước file quá lớn (tối đa 5MB)");
        $(event.target).val("");
        return;
      }

      UI.showFilePreview(file);

      // Enable the send button if a file is selected
      $(_config.selectors.sendButton).prop("disabled", false);
    }

    return {
      loadUsersWithChatHistory,
      loadChatHistory,
      markAsRead,
      sendMessage,
      editMessage,
      saveEditedMessage,
      cancelEditMessage,
      deleteMessage,
      showDiagnostics,
      testApiConnection,
      handleFileSelection,
    };
  })();

  /**
   * Event Handlers - Setup DOM event handlers
   */
  const EventHandlers = (function () {
    function initialize() {
      // Initial setup for message form
      setupMessageForm();

      // Set up file attachment handling
      setupFileHandling();

      // Set up keyboard shortcuts
      setupKeyboardShortcuts();

      // Set up search functionality
      setupSearch();
    }

    function setupMessageForm() {
      // Update message form HTML
      $(_config.selectors.messageForm).html(`
        <div class="message-input-container">
          <div id="file-preview-container"></div>
          <div class="message-input-wrapper">
            <button type="button" id="emoji-button" class="btn btn-light btn-sm">
              <i class="far fa-smile"></i>
            </button>
            <input
              type="text"
              id="chat-input"
              class="form-control"
              placeholder="Nhập tin nhắn..."
              disabled
            />
            <button type="button" id="attachment-button" class="btn btn-light btn-sm">
              <i class="fas fa-paperclip"></i>
            </button>
            <button
              type="submit"
              id="send-button"
              class="btn btn-primary btn-sm"
              disabled
            >
              <i class="fas fa-paper-plane"></i>
            </button>
          </div>
        </div>
      `);

      // Message form submit
      $(_config.selectors.messageForm).on("submit", function (e) {
        e.preventDefault();

        if (!_state.selectedUserId) return;

        const content = $(_config.selectors.chatInput).val().trim();
        const hasFile =
          $("#file-input").length &&
          $("#file-input")[0].files &&
          $("#file-input")[0].files.length > 0;

        // Must have either text content or file
        if (!content && !hasFile) return;

        // Create FormData object
        const formData = new FormData();
        formData.append("SenderId", _state.adminId);
        formData.append("ReceiverId", _state.selectedUserId);
        formData.append("Message", content);

        // Add file if exists
        if (hasFile) {
          formData.append("Attachment", $("#file-input")[0].files[0]);
        }

        ChatManager.sendMessage(formData);
      });

      // Initialize emoji picker
      $(document).on("click", "#emoji-button", function (e) {
        e.preventDefault();
        if (!_state.selectedUserId) return;

        // Check if emoji picker already exists
        if ($("#emoji-picker").length === 0) {
          // Create emoji picker container
          const emojiPicker = $("<div>", {
            id: "emoji-picker",
            class: "emoji-picker",
          });

          // Common emojis
          const commonEmojis = [
            "😀",
            "😁",
            "😂",
            "🤣",
            "😃",
            "😄",
            "😅",
            "😆",
            "😉",
            "😊",
            "😋",
            "😎",
            "😍",
            "😘",
            "😗",
            "😙",
            "😚",
            "🙂",
            "🤗",
            "🤔",
            "👍",
            "👎",
            "👌",
            "✌",
            "🤞",
            "👏",
            "🙌",
            "👋",
            "❤",
            "💔",
            "💯",
            "✨",
            "🔥",
            "🎉",
            "👀",
            "🤦‍♂️",
            "🤷‍♀️",
            "👑",
            "💪",
            "👏",
          ];

          // Add emojis to picker
          commonEmojis.forEach((emoji) => {
            const emojiBtn = $("<button>", {
              type: "button",
              class: "emoji-btn",
              text: emoji,
            });

            emojiBtn.on("click", function () {
              const input = $(_config.selectors.chatInput);
              input.val(input.val() + emoji);
              input.focus();
              emojiPicker.hide();
            });

            emojiPicker.append(emojiBtn);
          });

          // Add picker to DOM
          emojiPicker.insertAfter($(this));

          // Close picker when clicking outside
          $(document).on("click", function (event) {
            if (
              !$(event.target).closest("#emoji-button, #emoji-picker").length
            ) {
              emojiPicker.hide();
            }
          });
        } else {
          // Toggle picker visibility
          $("#emoji-picker").toggle();
        }
      });
    }

    function setupFileHandling() {
      // File attachment button click
      $(document).on("click", "#attachment-button", function (e) {
        e.preventDefault();
        // Create hidden file input if it doesn't exist
        if (!$("#file-input").length) {
          $(
            '<input type="file" id="file-input" style="display:none;">'
          ).appendTo("body");
        }

        // Trigger file selection dialog
        $("#file-input").trigger("click");
      });

      // File selection handler
      $(document).on("change", "#file-input", function (e) {
        ChatManager.handleFileSelection(e);
      });
    }

    function setupKeyboardShortcuts() {
      // Enter key to send message, shift+enter for new line
      $(_config.selectors.chatInput).on("keydown", function (e) {
        if (e.key === "Enter" && !e.shiftKey) {
          e.preventDefault();
          $(_config.selectors.messageForm).submit();
        }
      });

      // Enable/disable send button based on input content
      $(_config.selectors.chatInput).on("input", function () {
        const isEmpty = $(this).val().trim() === "";
        $(_config.selectors.sendButton).prop(
          "disabled",
          isEmpty && $("#file-input").length === 0
        );
      });
    }

    function setupSearch() {
      // Search functionality
      $(_config.selectors.searchUsers).on("input", function () {
        const searchTerm = $(this).val().toLowerCase();
        if (!searchTerm) {
          UI.renderUserList(_state.cache.usersWithChat);
          return;
        }

        const filteredUsers = _state.cache.usersWithChat.filter(
          (user) =>
            user.name.toLowerCase().includes(searchTerm) ||
            user.email.toLowerCase().includes(searchTerm)
        );

        UI.renderUserList(filteredUsers);
      });
    }

    return {
      initialize,
    };
  })();

  /**
   * Style Injector - Add custom CSS for chat components
   */
  const StyleInjector = (function () {
    function injectStyles() {
      $("<style>")
        .prop("type", "text/css")
        .html(
          `
          /* Container fixes */
          #message-container {
            height: 80vh;
            max-height: 800px;
            min-height: 500px;
            display: flex;
            flex-direction: column;
            overflow: hidden;
          }
          
          /* Overall chat layout */
          #admin-chat-container {
            display: flex;
            height: 100%;
            border-radius: 10px;
            overflow: hidden;
            box-shadow: 0 0 15px rgba(0, 0, 0, 0.1);
            background-color: #fff;
            flex: 1;
          }
          
          #users-list-container {
            width: 320px;
            border-right: 1px solid #e4e4e4;
            background-color: #f5f7fa;
            overflow-y: auto;
            height: 100%;
          }
          
          #chat-content-container {
            flex: 1;
            display: flex;
            flex-direction: column;
            background-color: #fff;
            height: 100%;
          }
          
          #chat-header {
            padding: 15px;
            border-bottom: 1px solid #e4e4e4;
            background-color: #f8f9fa;
            display: flex;
            align-items: center;
            justify-content: space-between;
          }
          
          #chat-messages {
            flex: 1;
            overflow-y: auto;
            padding: 15px;
            background-color: #f0f2f5;
            display: flex;
            flex-direction: column;
          }
          
          #message-form {
            padding: 10px 15px;
            border-top: 1px solid #e4e4e4;
            background-color: #fff;
          }
          
          /* Chat user list */
          .chat-user {
            display: flex;
            align-items: center;
            padding: 12px 15px;
            border-bottom: 1px solid #e4e4e4;
            cursor: pointer;
            transition: background-color 0.2s;
          }
          
          .chat-user:hover {
            background-color: #eaebef;
          }
          
          .chat-user.active {
            background-color: #e6f2ff;
            border-left: 3px solid #0084ff;
          }
          
          .chat-user-avatar {
            position: relative;
            width: 48px;
            height: 48px;
            margin-right: 12px;
            border-radius: 50%;
            overflow: hidden;
          }
          
          .chat-user-avatar img {
            width: 100%;
            height: 100%;
            object-fit: cover;
          }
          
          .chat-user-info {
            flex: 1;
            min-width: 0;
          }
          
          .chat-user-name {
            font-weight: 600;
            margin-bottom: 3px;
            white-space: nowrap;
            overflow: hidden;
            text-overflow: ellipsis;
          }
          
          .chat-user-status {
            font-size: 12px;
            color: #65676b;
            white-space: nowrap;
            overflow: hidden;
            text-overflow: ellipsis;
          }
          
          /* Message bubbles */
          .chat-message {
            display: flex;
            flex-direction: column;
            max-width: 70%;
            margin-bottom: 12px;
            position: relative;
          }
          
          .chat-message.user {
            align-self: flex-start;
            margin-right: auto;
          }
          
          .chat-message.admin {
            align-self: flex-end;
            margin-left: auto;
          }
          
          .message-bubble {
            padding: 10px 15px;
            border-radius: 18px;
            position: relative;
            word-wrap: break-word;
          }
          
          .chat-message.user .message-bubble {
            background-color: #f1f0f0;
            color: #050505;
            border-top-left-radius: 4px;
          }
          
          .chat-message.admin .message-bubble {
            background-color: #0084ff;
            color: white;
            border-top-right-radius: 4px;
          }
          
          .message-time-wrapper {
            margin-top: 2px;
            font-size: 11px;
            color: #65676b;
          }
          
          .chat-message.user .message-time-wrapper {
            margin-left: 5px;
          }
          
          .chat-message.admin .message-time-wrapper {
            margin-right: 5px;
            text-align: right;
          }
          
          /* Date separators */
          .date-separator {
            padding: 3px 10px;
            border-radius: 12px;
            font-size: 12px;
            color: #65676b;
            background-color: #f0f2f5;
            display: inline-block;
          }
          
          /* File attachments */
          .attachment-image img {
            max-width: 100%;
            max-height: 300px;
            border-radius: 12px;
            margin-top: 5px;
          }
          
          .attachment-file {
            display: flex;
            margin-top: 5px;
            background-color: rgba(0, 0, 0, 0.05);
            border-radius: 8px;
            padding: 8px;
            align-items: center;
          }
          
          .chat-message.admin .attachment-file {
            background-color: rgba(255, 255, 255, 0.2);
          }
          
          .file-icon {
            margin-right: 10px;
            color: #65676b;
          }
          
          .chat-message.admin .file-icon {
            color: white;
          }
          
          .file-info {
            flex: 1;
          }
          
          .file-name {
            font-size: 13px;
            font-weight: 500;
            word-break: break-all;
          }
          
          .file-link {
            text-decoration: none;
            color: inherit;
            display: flex;
            align-items: center;
          }
          
          /* File preview */
          .file-preview-container {
            padding: 5px;
            margin-bottom: 10px;
            background-color: #f7f7f7;
            border-radius: 10px;
            position: relative;
            max-width: 100%;
            overflow: hidden;
          }
          
          .file-preview-container img {
            max-height: 150px;
            max-width: 100%;
            border-radius: 8px;
            object-fit: contain;
          }
          
          .remove-file {
            position: absolute;
            top: 5px;
            right: 5px;
            width: 20px;
            height: 20px;
            background-color: rgba(0, 0, 0, 0.5);
            color: white;
            border-radius: 50%;
            display: flex;
            align-items: center;
            justify-content: center;
            cursor: pointer;
          }
          
          /* Message input and buttons */
          .message-input-container {
            width: 100%;
          }
          
          .message-input-wrapper {
            display: flex;
            align-items: center;
            background-color: #f0f2f5;
            border-radius: 20px;
            padding: 5px 10px;
          }
          
          #chat-input {
            flex: 1;
            border: none;
            background: transparent;
            padding: 8px 12px;
            outline: none !important;
            box-shadow: none !important;
            min-width: 0;
            overflow: hidden;
            text-overflow: ellipsis;
            white-space: nowrap;
          }
          
          #emoji-button, #attachment-button {
            width: 36px;
            height: 36px;
            border-radius: 50%;
            display: flex;
            align-items: center;
            justify-content: center;
            background-color: transparent;
            color: #65676b;
            border: none;
            margin: 0 3px;
            transition: background-color 0.2s;
            flex-shrink: 0;
          }
          
          #emoji-button:hover, #attachment-button:hover {
            background-color: #e4e6eb;
          }
          
          #send-button {
            width: 36px;
            height: 36px;
            border-radius: 50%;
            display: flex;
            align-items: center;
            justify-content: center;
            background-color: #0084ff;
            color: white;
            border: none;
            margin-left: 5px;
            transition: background-color 0.2s;
            flex-shrink: 0;
          }
          
          #send-button:hover {
            background-color: #0070da;
          }
          
          #send-button:disabled {
            background-color: #e4e6eb;
            color: #bcc0c4;
          }
          
          /* Emoji picker */
          .emoji-picker {
            position: absolute;
            bottom: 60px;
            left: 20px;
            width: 240px;
            background-color: white;
            border-radius: 8px;
            box-shadow: 0 2px 10px rgba(0, 0, 0, 0.15);
            padding: 8px;
            display: flex;
            flex-wrap: wrap;
            z-index: 1000;
            max-height: 200px;
            overflow-y: auto;
          }
          
          .emoji-btn {
            width: 34px;
            height: 34px;
            font-size: 22px;
            background: none;
            border: none;
            cursor: pointer;
            border-radius: 4px;
          }
          
          .emoji-btn:hover {
            background-color: #f1f3f5;
          }
          
          /* Unread count */
          .chat-user-unread {
            background-color: #0084ff;
            color: white;
            font-size: 12px;
            font-weight: 600;
            min-width: 20px;
            height: 20px;
            border-radius: 10px;
            display: flex;
            align-items: center;
            justify-content: center;
            padding: 0 6px;
          }
          
          /* Message actions */
          .message-actions {
            position: absolute;
            top: 5px;
            right: 5px;
            visibility: hidden;
            opacity: 0;
            transition: opacity 0.2s ease, visibility 0.2s;
            display: flex;
            gap: 6px;
            z-index: 100;
          }
          
          .chat-message:hover .message-bubble {
            padding-right: 40px;
          }
          
          .chat-message:hover .message-actions {
            visibility: visible;
            opacity: 1;
          }
          
          .message-actions button {
            width: 24px;
            height: 24px;
            padding: 0;
            border: none;
            border-radius: 50%;
            display: flex;
            align-items: center;
            justify-content: center;
            background-color: rgba(255, 255, 255, 0.9);
            box-shadow: 0 1px 3px rgba(0, 0, 0, 0.1);
            color: #555;
            transition: all 0.2s ease;
          }
          
          .message-actions .edit-btn:hover {
            background-color: #e3f2fd;
            color: #007bff;
            transform: scale(1.1);
          }
          
          .message-actions .delete-btn:hover {
            background-color: #ffebee;
            color: #dc3545;
            transform: scale(1.1);
          }
          
          /* Other styling */
          .chat-section-header {
            padding: 8px 15px;
            background-color: #f5f7fa;
            font-size: 13px;
            font-weight: 600;
            color: #65676b;
            text-transform: uppercase;
            letter-spacing: 0.5px;
          }
          
          .empty-chat-state {
            padding: 50px 20px;
            color: #8e8e8e;
          }
          
          /* Message editing */
          .edit-container {
            width: 100%;
          }
          
          .edit-input {
            width: 100%;
            resize: none;
            font-size: 14px;
            border: 1px solid #ddd;
            border-radius: 8px;
            padding: 8px;
            margin-bottom: 5px;
          }
          
          .edit-actions {
            display: flex;
            justify-content: flex-end;
            gap: 5px;
          }
          
          /* Animation */
          .fade-out {
            animation: fadeOut 0.3s forwards;
          }
          
          @keyframes fadeOut {
            from { opacity: 1; }
            to { opacity: 0; height: 0; margin: 0; padding: 0; overflow: hidden; }
          }
          
          /* Unread indicator */
          .chat-user-avatar.has-unread::after {
            content: '';
            position: absolute;
            width: 12px;
            height: 12px;
            background-color: #0084ff;
            border-radius: 50%;
            border: 2px solid #f5f7fa;
            top: 0;
            right: 0;
          }
          
          /* Fix for messenger footer overflow */
          .messenger-content-footer {
            width: 100%;
            padding: 10px;
            box-sizing: border-box;
            border-top: 1px solid #e4e4e4;
            background-color: #fff;
            overflow: hidden;
            position: relative;
            z-index: 10;
            display: block;
            flex-shrink: 0;
          }
          
          .messenger-content-footer #message-form {
            width: 100%;
            margin: 0;
            padding: 0;
            overflow: hidden;
            display: block;
          }
          
          .messenger-content-footer .message-input-container {
            width: 100%;
            max-width: 100%;
            overflow: hidden;
            display: block;
          }
          
          .messenger-content-footer .message-input-wrapper {
            width: 100%;
            display: flex;
            flex-wrap: nowrap;
            max-width: 100%;
            align-items: center;
          }
          
          /* Fix messenger form layout */
          .messenger-content-footer {
            position: relative;
            bottom: 0;
            left: 0;
            right: 0;
            background-color: #fff;
            border-top: 1px solid #e4e4e4;
            padding: 10px;
            z-index: 200;
            flex-shrink: 0;
            height: auto;
            max-height: 200px;
            width: 100%;
            box-sizing: border-box;
            overflow: hidden;
          }
          
          /* Rest of styles */
          /* Message form fix */
          .message-form {
            display: block;
            width: 100%;
            margin: 0;
            padding: 0;
            overflow: hidden;
          }
          
          /* Fix the message input wrapper to not overflow */
          .message-input-wrapper {
            display: flex;
            align-items: center;
            background-color: #f0f2f5;
            border-radius: 20px;
            padding: 5px 8px;
            width: 100%;
            box-sizing: border-box;
            overflow: hidden;
          }
          
          /* Fix form max width overflow */
          form#message-form {
            max-width: 100% !important;
            width: 100% !important;
            overflow: hidden !important;
          }
          
          .messenger-content-footer form {
            max-width: 100%;
            overflow: hidden;
          }
          
          .message-input-container {
            max-width: 100% !important;
            overflow: hidden !important;
            width: 100% !important;
          }
          
          /* Fix input wrapper overflow */
          .messenger-content-footer .message-input-wrapper {
            width: 100%;
            max-width: 100%;
            box-sizing: border-box;
            display: flex;
            align-items: center;
            background-color: #f0f2f5;
            border-radius: 20px;
            padding: 5px 8px;
            overflow: hidden;
            margin: 0;
          }
          
          /* Ensure footer doesn't let content overflow */
          .messenger-content-footer {
            display: flex;
            flex-direction: column;
            padding: 10px;
            box-sizing: border-box;
            width: 100%;
            max-width: 100%;
            overflow: hidden;
            position: relative;
            z-index: 10;
            flex-shrink: 0;
            background-color: #fff;
            border-top: 1px solid #e4e4e4;
          }
          
          /* Constrain inputs within wrapper */
          .message-input-wrapper input,
          .message-input-wrapper button {
            flex-shrink: 0;
            max-width: 100%;
            box-sizing: border-box;
          }
        `
        )
        .appendTo("head");
    }

    return {
      injectStyles,
    };
  })();

  /**
   * Initialize the admin chat module
   */
  function init() {
    Utils.log("info", "Initializing Admin Chat module");

    // Get admin ID from DOM
    const adminChatElement = document.getElementById("admin-chat");
    _state.adminId = adminChatElement.dataset.adminId;

    // Use the message-container instead of admin-chat
    const container = $("#message-container");

    // Add chat UI to message-container
    container.html(`
      <div id="admin-chat-container">
        <div id="users-list-container">
          <div class="p-3">
            <div class="input-group">
              <span class="input-group-text bg-light border-0">
                <i class="fas fa-search text-muted"></i>
              </span>
              <input type="text" id="search-users" class="form-control bg-light border-0" placeholder="Tìm kiếm người dùng...">
            </div>
          </div>
          <div id="users-list"></div>
        </div>
        <div id="chat-content-container">
          <div id="chat-header">
            <div class="d-flex align-items-center">
              <div class="fw-bold" id="selected-user-name">Quản lý tin nhắn</div>
              <div class="ms-2 badge bg-light text-dark"><span id="total-users-count">0</span> người dùng</div>
            </div>
            <div>
              <button class="btn btn-sm btn-outline-secondary" onclick="AdminChat.loadUsersWithChatHistory(true)">
                <i class="fas fa-sync-alt"></i>
              </button>
            </div>
          </div>
          <div id="no-chat-selected" class="flex-grow-1 d-flex align-items-center justify-content-center">
            <div class="text-center text-muted">
              <i class="far fa-comment-dots fa-4x mb-3"></i>
              <h5>Chọn người dùng để bắt đầu cuộc trò chuyện</h5>
              <p>Chưa có cuộc hội thoại nào</p>
            </div>
          </div>
          <div id="chat-area" class="d-none flex-grow-1 d-flex flex-column">
            <div id="chat-messages" class="flex-grow-1"></div>
            <div class="messenger-content-footer">
              <form id="message-form" class="message-form"></form>
            </div>
          </div>
        </div>
      </div>
    `);

    // Remove the admin-chat element to prevent duplication
    $(adminChatElement).remove();

    // Initialize SignalR connection
    SignalRService.initialize().then(() => {
      // Load users with chat history
      ChatManager.loadUsersWithChatHistory();
    });

    // Initialize event handlers
    EventHandlers.initialize();

    // Inject styles
    StyleInjector.injectStyles();
  }

  // Public API
  return {
    init,
    loadUsersWithChatHistory: ChatManager.loadUsersWithChatHistory,
    loadChatHistory: ChatManager.loadChatHistory,
    editMessage: ChatManager.editMessage,
    deleteMessage: ChatManager.deleteMessage,
    showDiagnostics: ChatManager.showDiagnostics,
    testApiConnection: ChatManager.testApiConnection,
  };
})();

// Initialize when document is ready
$(document).ready(function () {
  AdminChat.init();
});
