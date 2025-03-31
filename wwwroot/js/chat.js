// chat.js - Xử lý chức năng chat
class ChatWidget {
  constructor() {
    this.connection = null;
    this.isOpen = false;
    this.userId = document.getElementById("chat-widget").dataset.userId;
    this.adminId = document.getElementById("chat-widget").dataset.adminId;

    // Add CSS styles
    const style = document.createElement("style");
    style.textContent = `
      .message-actions {
          display: flex;
          gap: 0.5rem;
          opacity: 0;
          transition: opacity 0.2s;
      }

      .chat-message:hover .message-actions {
          opacity: 1;
      }

      .message-actions button {
          padding: 0;
          font-size: 0.875rem;
      }

      .edit-message-input {
          margin: 0.5rem 0;
      }

      .edit-actions {
          display: flex;
          gap: 0.5rem;
      }
    `;
    document.head.appendChild(style);

    console.log("Chat widget initialized");
    console.log("User ID:", this.userId);
    console.log("Admin ID:", this.adminId);

    this.initializeEventListeners();
    this.initialize();
  }

  initializeEventListeners() {
    // Toggle chat widget
    document
      .getElementById("chat-toggle-button")
      .addEventListener("click", () => {
        console.log("Chat toggle button clicked");
        this.isOpen = !this.isOpen;
        document.getElementById("chat-widget").classList.toggle("open");
        document
          .getElementById("chat-toggle-button")
          .classList.toggle("hidden");

        if (this.isOpen) {
          this.markAsRead();
          document.getElementById("chat-messages").scrollTop =
            document.getElementById("chat-messages").scrollHeight;
        }
      });

    // Close chat widget
    document.getElementById("toggle-chat").addEventListener("click", () => {
      console.log("Close button clicked");
      this.isOpen = false;
      document.getElementById("chat-widget").classList.remove("open");
      document.getElementById("chat-toggle-button").classList.remove("hidden");
    });

    // Send message
    document
      .getElementById("send-message")
      .addEventListener("click", () => this.sendMessage());
    document
      .getElementById("message-input")
      .addEventListener("keypress", (e) => {
        if (e.which === 13) {
          this.sendMessage();
        }
      });
  }

  async initialize() {
    if (!this.userId || !this.adminId) {
      console.error("Missing user or admin ID, chat functionality disabled");
      if (!this.adminId) {
        console.log("Admin ID not provided, fetching from API");
        try {
          const response = await fetch("/api/Chat/admin-id");
          if (!response.ok) {
            throw new Error("Failed to get admin ID");
          }
          const data = await response.json();
          if (data && data.adminId) {
            this.adminId = data.adminId;
            this.initializeSignalR();
          } else {
            console.error("No admin ID returned from API");
          }
        } catch (error) {
          console.error("Error fetching admin ID:", error);
        }
      }
      return;
    }

    this.initializeSignalR();

    // Check for new messages every minute
    setInterval(() => {
      if (!this.isOpen) {
        this.getUnreadCount();
      }
    }, 60000);
  }

  initializeSignalR() {
    console.log("Initializing SignalR connection");
    this.connection = new signalR.HubConnectionBuilder()
      .withUrl("/chatHub")
      .withAutomaticReconnect()
      .build();

    this.setupSignalRHandlers();

    this.connection
      .start()
      .then(() => {
        console.log("SignalR Connected");
        // Join user's group
        this.connection.invoke("JoinGroup", this.userId).catch(function (err) {
          console.error(err);
        });

        this.loadChatHistory();
        this.getUnreadCount();
      })
      .catch(function (err) {
        console.error("Error connecting to SignalR:", err);
      });
  }

  setupSignalRHandlers() {
    this.connection.on("ReceiveMessage", (message) => {
      console.log("Message received:", message);
      this.addMessageToChat(message);

      const isFromUser = message.senderId === this.userId;
      if (!isFromUser && this.isOpen) {
        this.markAsRead();
      }
      if (!isFromUser && !this.isOpen) {
        this.incrementUnreadCount();
      }
    });

    this.connection.on("MessageUpdated", (message) => {
      const messageElement = document.querySelector(
        `[data-message-id="${message.id}"]`
      );
      if (messageElement) {
        messageElement.querySelector(".message-text").textContent =
          message.content;
      }
    });

    this.connection.on("MessageDeleted", (messageId) => {
      const messageElement = document.querySelector(
        `[data-message-id="${messageId}"]`
      );
      if (messageElement) {
        messageElement.remove();
      }
    });
  }

  addMessageToChat(message) {
    const isFromUser = message.senderId === this.userId;
    const messageTime = new Date(message.timestamp);
    const canEdit = this.canEditMessage(messageTime);
    const canDelete = this.canDeleteMessage(messageTime);

    const messageHtml = `
        <div class="message ${
          isFromUser ? "message-sent" : "message-received"
        }" 
             data-message-id="${message.id}"
             data-timestamp="${message.timestamp}">
            <div class="message-content">
                <div class="message-text">${message.content}</div>
            </div>
            ${
              isFromUser
                ? `
                <div class="message-actions">
                    ${
                      canEdit
                        ? `
                        <button class="btn btn-sm btn-link edit-btn" 
                                onclick="window.chatWidget.editMessage(${message.id})" 
                                title="Sửa tin nhắn">
                            <i class="bi bi-pencil-fill"></i>
                        </button>
                    `
                        : ""
                    }
                    ${
                      canDelete
                        ? `
                        <button class="btn btn-sm btn-link delete-btn" 
                                onclick="window.chatWidget.deleteMessage(${message.id})" 
                                title="Xóa tin nhắn">
                            <i class="bi bi-trash-fill"></i>
                        </button>
                    `
                        : ""
                    }
                </div>
            `
                : ""
            }
            <small class="message-time">
                ${new Date(message.timestamp).toLocaleTimeString()}
                ${!canEdit && isFromUser ? " (Không thể chỉnh sửa)" : ""}
            </small>
            </div>
        `;

    const emptyState = document.querySelector(".chat-empty-state");
    if (emptyState) {
      emptyState.remove();
    }

    document
      .getElementById("chat-messages")
      .insertAdjacentHTML("beforeend", messageHtml);
    document.getElementById("chat-messages").scrollTop =
      document.getElementById("chat-messages").scrollHeight;
  }

  async sendMessage() {
    const content = document.getElementById("message-input").value.trim();
    if (content && this.connection) {
      console.log("Sending message:", content);
      const message = {
        senderId: this.userId,
        receiverId: this.adminId,
        message: content,
      };

      try {
        const response = await fetch("/api/Chat/send", {
          method: "POST",
          headers: {
            "Content-Type": "application/json",
          },
          body: JSON.stringify(message),
        });

        if (!response.ok) {
          throw new Error("Network response was not ok");
        }
        console.log("Message sent successfully");
        document.getElementById("message-input").value = "";
      } catch (error) {
        console.error("Error sending message", error);
        toastr.error("Không thể gửi tin nhắn");
      }
    }
  }

  async loadChatHistory() {
    console.log("Loading chat history");
    try {
      const response = await fetch(
        `/api/Chat/history?userId=${this.userId}&otherUserId=${this.adminId}`
      );
      if (!response.ok) {
        throw new Error("Network response was not ok");
      }
      const messages = await response.json();
      console.log("Chat history loaded:", messages);

      const chatMessages = document.getElementById("chat-messages");
      chatMessages.innerHTML = "";

      if (messages.length === 0) {
        chatMessages.innerHTML = `
                    <div class="chat-empty-state">
                        <i class="bi bi-chat-square-text"></i>
                        <p>No messages yet</p>
                        <p class="text-muted">Start a conversation with our team!</p>
                    </div>
                `;
        return;
      }

      messages.forEach((message) => this.addMessageToChat(message));
    } catch (error) {
      console.error("Error loading chat history", error);
      const chatMessages = document.getElementById("chat-messages");
      chatMessages.innerHTML = `
                <div class="chat-empty-state">
                    <i class="bi bi-exclamation-circle"></i>
                    <p>Failed to load messages</p>
                    <p class="text-muted">Please try again later.</p>
                </div>
            `;
    }
  }

  async markAsRead() {
    console.log("Marking messages as read");
    const data = {
      senderId: this.adminId,
      receiverId: this.userId,
    };

    try {
      const response = await fetch("/api/Chat/mark-as-read", {
        method: "POST",
        headers: {
          "Content-Type": "application/json",
        },
        body: JSON.stringify(data),
      });

      if (!response.ok) {
        throw new Error("Network response was not ok");
      }

      const unreadCount = document.getElementById("unread-count");
      unreadCount.textContent = "0";
      unreadCount.style.display = "none";
    } catch (error) {
      console.error("Error marking messages as read", error);
    }
  }

  async getUnreadCount() {
    console.log("Getting unread count");
    try {
      const response = await fetch(
        `/api/Chat/unread-count?userId=${this.userId}`
      );
      if (!response.ok) {
        throw new Error("Network response was not ok");
      }
      const data = await response.json();
      let count = typeof data === "object" ? data.count : data;
      console.log("Unread count:", count);

      const unreadCount = document.getElementById("unread-count");
      if (count > 0) {
        unreadCount.textContent = count;
        unreadCount.style.display = "block";
      } else {
        unreadCount.style.display = "none";
      }
    } catch (error) {
      console.error("Error getting unread count", error);
    }
  }

  incrementUnreadCount() {
    const unreadCount = document.getElementById("unread-count");
    const current = parseInt(unreadCount.textContent) || 0;
    unreadCount.textContent = current + 1;
    unreadCount.style.display = "block";
  }

  canEditMessage(messageTime) {
    const now = new Date();
    const hoursDiff = (now - messageTime) / (1000 * 60 * 60);
    return hoursDiff <= 1;
  }

  canDeleteMessage(messageTime) {
    // Người dùng luôn có thể xóa tin nhắn ở phía họ
    return true;
  }

  editMessage(messageId) {
    const messageElement = document.querySelector(
      `[data-message-id="${messageId}"]`
    );
    const timestamp = new Date(messageElement.dataset.timestamp);

    if (!this.canEditMessage(timestamp)) {
      toastr.error("Không thể chỉnh sửa tin nhắn sau 1 giờ");
      return;
    }

    // Thêm class editing để áp dụng style
    messageElement.classList.add("editing");

    const messageText =
      messageElement.querySelector(".message-text").textContent;

    // Tạo container cho phần chỉnh sửa
    const editContainer = document.createElement("div");
    editContainer.className = "message-edit-container";

    // Thêm header
    const editHeader = document.createElement("div");
    editHeader.className = "message-edit-header";
    editHeader.innerHTML =
      '<i class="bi bi-pencil-square"></i> Đang chỉnh sửa tin nhắn';

    // Tạo input
    const editInput = document.createElement("input");
    editInput.type = "text";
    editInput.value = messageText;
    editInput.className = "form-control edit-message-input";
    editInput.placeholder = "Nhập nội dung tin nhắn mới...";

    // Tạo khung chứa các nút
    const editActions = document.createElement("div");
    editActions.className = "edit-actions";
    editActions.innerHTML = `
        <button class="cancel-btn" onclick="window.chatWidget.cancelEdit(${messageId})">
            <i class="bi bi-x"></i>
            <span>Hủy</span>
        </button>
        <button class="save-btn" onclick="window.chatWidget.saveEdit(${messageId})">
            <i class="bi bi-check2"></i>
            <span>Lưu</span>
        </button>
    `;

    // Ẩn nội dung tin nhắn gốc
    messageElement.querySelector(".message-text").style.display = "none";
    messageElement.querySelector(".message-actions").style.display = "none";

    // Thêm các phần tử mới
    editContainer.appendChild(editHeader);
    editContainer.appendChild(editInput);
    editContainer.appendChild(editActions);
    messageElement.insertBefore(
      editContainer,
      messageElement.querySelector(".message-time")
    );

    // Focus vào input
    editInput.focus();

    // Thêm xử lý phím Enter để lưu
    editInput.addEventListener("keypress", (e) => {
      if (e.key === "Enter") {
        this.saveEdit(messageId);
      }
    });
  }

  async saveEdit(messageId) {
    const messageElement = document.querySelector(
      `[data-message-id="${messageId}"]`
    );
    const editInput = messageElement.querySelector(".edit-message-input");
    const newText = editInput.value.trim();

    if (!newText) {
      toastr.error("Tin nhắn không được để trống");
      return;
    }

    try {
      const response = await fetch(`/api/chat/messages/${messageId}`, {
        method: "PUT",
        headers: {
          "Content-Type": "application/json",
        },
        body: JSON.stringify({ content: newText }),
      });

      if (response.ok) {
        messageElement.querySelector(".message-text").textContent = newText;
        this.cancelEdit(messageId);
        toastr.success("Đã cập nhật tin nhắn");
      } else {
        toastr.error("Không thể cập nhật tin nhắn");
      }
    } catch (error) {
      console.error("Error updating message:", error);
      toastr.error("Đã xảy ra lỗi khi cập nhật tin nhắn");
    }
  }

  cancelEdit(messageId) {
    const messageElement = document.querySelector(
      `[data-message-id="${messageId}"]`
    );

    // Xóa class editing
    messageElement.classList.remove("editing");

    // Hiện lại nội dung tin nhắn gốc
    messageElement.querySelector(".message-text").style.display = "block";
    messageElement.querySelector(".message-actions").style.display = "flex";

    // Xóa container chỉnh sửa
    const editContainer = messageElement.querySelector(
      ".message-edit-container"
    );
    if (editContainer) {
      editContainer.remove();
    }
  }

  async deleteMessage(messageId) {
    if (!confirm("Bạn có chắc chắn muốn xóa tin nhắn này?")) {
      return;
    }

    // Tìm phần tử tin nhắn trước khi gọi API
    const messageElement = document.querySelector(
      `[data-message-id="${messageId}"]`
    );
    if (!messageElement) {
      console.error("Message element not found:", messageId);
      toastr.error("Không tìm thấy tin nhắn để xóa");
      return;
    }

    try {
      const response = await fetch(`/api/chat/messages/${messageId}`, {
        method: "DELETE",
      });

      if (response.ok) {
        // Xóa phần tử tin nhắn khỏi DOM
        messageElement.remove();
        toastr.success("Đã xóa tin nhắn");
      } else {
        const errorData = await response.json().catch(() => ({}));
        console.error("Server error:", errorData);
        toastr.error(errorData.message || "Không thể xóa tin nhắn");
      }
    } catch (error) {
      console.error("Error deleting message:", error);
      toastr.error("Đã xảy ra lỗi khi xóa tin nhắn");
    }
  }
}

// Initialize the chat widget when the DOM is loaded
document.addEventListener("DOMContentLoaded", function () {
  const chatWidgetInstance = new ChatWidget();
  window.chatWidget = chatWidgetInstance;
});
