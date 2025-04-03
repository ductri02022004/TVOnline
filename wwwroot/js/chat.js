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
          position: absolute;
          top: 5px;
          right: 5px;
      }

      .message-sent:hover .message-actions {
          opacity: 1;
      }

      .message-actions button {
          padding: 0;
          font-size: 0.875rem;
          background: rgba(255, 255, 255, 0.8);
          border: none;
          width: 24px;
          height: 24px;
          border-radius: 50%;
          display: flex;
          align-items: center;
          justify-content: center;
      }

      .edit-message-input {
          margin: 0.5rem 0;
      }

      .edit-actions {
          display: flex;
          gap: 0.5rem;
      }
      
      .emoji-picker {
          position: absolute;
          bottom: 60px;
          left: 10px;
          background-color: white;
          border: 1px solid #ddd;
          border-radius: 5px;
          padding: 10px;
          display: grid;
          grid-template-columns: repeat(8, 1fr);
          gap: 5px;
          max-width: 300px;
          z-index: 100;
          box-shadow: 0 0 10px rgba(0, 0, 0, 0.1);
      }
      
      .message {
          position: relative;
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

    // Emoji button
    document
      .getElementById("emoji-button")
      .addEventListener("click", (e) => this.toggleEmojiPicker(e));

    // Attachment button
    document
      .getElementById("attachment-button")
      .addEventListener("click", () => this.openFileSelector());

    // Close emoji picker when clicking outside
    document.addEventListener("click", (e) => {
      const emojiPicker = document.getElementById("emoji-picker");
      const emojiButton = document.getElementById("emoji-button");

      if (
        emojiPicker &&
        !emojiPicker.contains(e.target) &&
        e.target !== emojiButton
      ) {
        emojiPicker.remove();
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

    // Check if message contains a file attachment
    let messageContent = message.content;
    let fileAttachment = "";

    if (message.content && message.content.includes("[FILE]")) {
      const contentParts = message.content.split("\n[FILE]");
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
        let iconClass = "bi bi-file-earmark";
        if (fileExt.includes(".pdf")) {
          iconClass = "bi bi-file-earmark-pdf";
        } else if (fileExt.includes(".doc")) {
          iconClass = "bi bi-file-earmark-word";
        } else if (fileExt.includes(".xls")) {
          iconClass = "bi bi-file-earmark-excel";
        } else if (fileExt.includes(".txt")) {
          iconClass = "bi bi-file-earmark-text";
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

    const messageHtml = `
        <div class="message ${
          isFromUser ? "message-sent" : "message-received"
        }" 
             data-message-id="${message.id}"
             data-timestamp="${message.timestamp}">
            <div class="message-content">
                <div class="message-text">${messageContent}</div>
                ${fileAttachment}
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
    const fileInput = document.getElementById("file-input");
    const hasFile = fileInput && fileInput.files && fileInput.files.length > 0;

    // Check if we have content or a file to send
    if ((!content && !hasFile) || !this.connection) {
      return;
    }

    // Get current user ID from the widget
    if (!this.userId || !this.adminId) {
      console.error("Missing user or admin ID");
      if (typeof toastr !== "undefined") {
        toastr.error("Lỗi: Không thể xác định ID người dùng");
      } else {
        alert("Lỗi: Không thể xác định ID người dùng");
      }
      return;
    }

    try {
      if (hasFile) {
        // We have a file to upload, use FormData
        const formData = new FormData();
        formData.append("SenderId", this.userId);
        formData.append("ReceiverId", this.adminId);
        formData.append("Message", content || fileInput.files[0].name);
        formData.append("Attachment", fileInput.files[0]);

        const response = await fetch("/api/Chat/send-with-attachment", {
          method: "POST",
          body: formData,
        });

        if (!response.ok) {
          let errorMsg = "Không thể gửi tin nhắn";
          try {
            const errorData = await response.json();
            errorMsg =
              errorData.message ||
              `Lỗi: ${response.status} ${response.statusText}`;
          } catch (e) {
            errorMsg = `Lỗi: ${response.status} ${response.statusText}`;
          }
          throw new Error(errorMsg);
        }

        // Clear file input and preview
        this.clearFilePreview();
      } else {
        // Text-only message
        const response = await fetch("/api/Chat/messages", {
          method: "POST",
          headers: {
            "Content-Type": "application/json",
          },
          body: JSON.stringify({
            SenderId: this.userId,
            ReceiverId: this.adminId,
            Message: content,
          }),
        });

        if (!response.ok) {
          let errorMsg = "Không thể gửi tin nhắn";
          try {
            const errorData = await response.json();
            errorMsg =
              errorData.message ||
              `Lỗi: ${response.status} ${response.statusText}`;
          } catch (e) {
            errorMsg = `Lỗi: ${response.status} ${response.statusText}`;
          }
          throw new Error(errorMsg);
        }
      }

      console.log("Message sent successfully");
      document.getElementById("message-input").value = "";
    } catch (error) {
      console.error("Error sending message", error);
      // Use alert instead of toastr if toastr is not available
      if (typeof toastr !== "undefined") {
        toastr.error(error.message || "Không thể gửi tin nhắn");
      } else {
        alert(error.message || "Không thể gửi tin nhắn");
      }
    }
  }

  // Toggle emoji picker
  toggleEmojiPicker(event) {
    event.stopPropagation();

    // Remove existing emoji picker if it exists
    const existingPicker = document.getElementById("emoji-picker");
    if (existingPicker) {
      existingPicker.remove();
      return;
    }

    // Create emoji picker
    const emojiList = [
      "😀",
      "😃",
      "😄",
      "😁",
      "😆",
      "😅",
      "🤣",
      "😂",
      "🙂",
      "🙃",
      "😉",
      "😊",
      "😇",
      "😍",
      "😘",
      "😗",
      "😙",
      "😚",
      "😋",
      "😛",
      "😜",
      "😝",
      "🤑",
      "🤗",
      "🤭",
      "🤫",
      "🤔",
      "🤐",
      "🤨",
      "😐",
      "😑",
      "😶",
      "😏",
      "😒",
      "🙄",
      "😬",
      "🤥",
      "😌",
      "😔",
      "😪",
      "👍",
      "👎",
      "👏",
      "🙏",
      "👋",
      "❤️",
      "🎉",
      "✅",
      "❌",
      "⭐",
    ];

    const emojiPicker = document.createElement("div");
    emojiPicker.id = "emoji-picker";
    emojiPicker.className = "emoji-picker";

    emojiList.forEach((emoji) => {
      const emojiSpan = document.createElement("span");
      emojiSpan.className = "emoji-item";
      emojiSpan.textContent = emoji;
      emojiSpan.onclick = () => this.insertEmoji(emoji);
      emojiPicker.appendChild(emojiSpan);
    });

    // Position the emoji picker relative to chat input area
    const chatInputArea = document.querySelector(".chat-input-area");
    chatInputArea.appendChild(emojiPicker);
  }

  // Insert emoji at cursor position
  insertEmoji(emoji) {
    const input = document.getElementById("message-input");
    const startPos = input.selectionStart;
    const endPos = input.selectionEnd;
    const text = input.value;
    const newText =
      text.substring(0, startPos) + emoji + text.substring(endPos);

    input.value = newText;
    input.focus();
    input.selectionStart = startPos + emoji.length;
    input.selectionEnd = startPos + emoji.length;
  }

  // File input handling
  openFileSelector() {
    const fileInput =
      document.getElementById("file-input") || this.createFileInput();
    fileInput.click();
  }

  createFileInput() {
    const fileInput = document.createElement("input");
    fileInput.type = "file";
    fileInput.id = "file-input";
    fileInput.style.display = "none";
    fileInput.accept = "image/*,.pdf,.doc,.docx,.xls,.xlsx,.txt";
    fileInput.addEventListener("change", (e) => this.handleFileSelected(e));
    document.body.appendChild(fileInput);
    return fileInput;
  }

  handleFileSelected(e) {
    const file = e.target.files[0];
    if (!file) {
      return;
    }

    // Check file size (5MB max)
    const maxSize = 5 * 1024 * 1024; // 5MB
    if (file.size > maxSize) {
      if (typeof toastr !== "undefined") {
        toastr.error("Kích thước file quá lớn (tối đa 5MB)");
      } else {
        alert("Kích thước file quá lớn (tối đa 5MB)");
      }
      e.target.value = "";
      return;
    }

    this.showFilePreview(file);
  }

  showFilePreview(file) {
    const previewContainer = document.getElementById("file-preview-container");
    previewContainer.innerHTML = ""; // Clear existing preview

    // Create preview element
    const previewElement = document.createElement("div");
    previewElement.className = "file-preview-container";

    // Check if file is an image
    const isImage = file.type.startsWith("image/");

    if (isImage) {
      const reader = new FileReader();
      reader.onload = function (e) {
        const img = document.createElement("img");
        img.src = e.target.result;
        previewElement.appendChild(img);
      };
      reader.readAsDataURL(file);
    } else {
      // Show appropriate icon based on file type
      let iconClass = "bi-file";
      const extension = file.name.split(".").pop().toLowerCase();

      if (["pdf"].includes(extension)) {
        iconClass = "bi-file-pdf";
      } else if (["doc", "docx"].includes(extension)) {
        iconClass = "bi-file-word";
      } else if (["xls", "xlsx"].includes(extension)) {
        iconClass = "bi-file-excel";
      } else if (["txt"].includes(extension)) {
        iconClass = "bi-file-text";
      }

      const icon = document.createElement("i");
      icon.className = `bi ${iconClass}`;
      icon.style.fontSize = "2rem";
      icon.style.color = "#6c757d";
      icon.style.marginRight = "10px";
      previewElement.appendChild(icon);
    }

    // Add file info
    const fileInfo = document.createElement("div");
    fileInfo.className = "file-info";

    const fileName = document.createElement("div");
    fileName.className = "file-name";
    fileName.textContent = file.name;

    const fileSize = document.createElement("div");
    fileSize.className = "file-size";
    fileSize.textContent = this.formatFileSize(file.size);

    fileInfo.appendChild(fileName);
    fileInfo.appendChild(fileSize);
    previewElement.appendChild(fileInfo);

    // Add remove button
    const removeBtn = document.createElement("div");
    removeBtn.className = "remove-file";
    removeBtn.innerHTML = '<i class="bi bi-x"></i>';
    removeBtn.addEventListener("click", () => this.clearFilePreview());
    previewElement.appendChild(removeBtn);

    previewContainer.appendChild(previewElement);
    previewContainer.style.display = "block";
  }

  clearFilePreview() {
    const fileInput = document.getElementById("file-input");
    if (fileInput) {
      fileInput.value = "";
    }

    const previewContainer = document.getElementById("file-preview-container");
    if (previewContainer) {
      previewContainer.innerHTML = "";
      previewContainer.style.display = "none";
    }
  }

  formatFileSize(bytes) {
    if (bytes < 1024) {
      return bytes + " B";
    } else if (bytes < 1024 * 1024) {
      return (bytes / 1024).toFixed(1) + " KB";
    } else {
      return (bytes / (1024 * 1024)).toFixed(2) + " MB";
    }
  }

  async loadChatHistory() {
    if (!this.userId || !this.adminId) {
      console.error("User IDs not set for chat history");
      return;
    }

    this.setLoadingState("Đang tải tin nhắn...");

    fetch(`/api/Chat/history?userId=${this.userId}&otherUserId=${this.adminId}`)
      .then((response) => {
        if (!response.ok) {
          throw new Error("Network response was not ok");
        }
        return response.json();
      })
      .then((data) => {
        this.clearMessages();
        if (Array.isArray(data) && data.length > 0) {
          data.forEach((message) => {
            this.addMessageToChat(message);
          });
          this.scrollToBottom();
        } else {
          this.setEmptyState();
        }
      })
      .catch((error) => {
        console.error("Error loading chat history", error);
        this.setErrorState(
          "Không thể tải tin nhắn",
          "Vui lòng thử lại sau.",
          () => this.loadChatHistory()
        );
      });
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

  // Add missing methods
  setLoadingState(message) {
    const chatMessagesEl = document.getElementById("chat-messages");
    chatMessagesEl.innerHTML = `
      <div class="text-center p-4 loading-state">
        <div class="spinner-border text-primary" role="status">
          <span class="visually-hidden">Đang tải...</span>
        </div>
        <p class="mt-2">${message || "Đang tải..."}</p>
      </div>
    `;
  }

  setEmptyState() {
    const chatMessagesEl = document.getElementById("chat-messages");
    chatMessagesEl.innerHTML = `
      <div class="text-center empty-chat-state chat-empty-state">
        <div class="empty-chat-icon">
          <i class="bi bi-chat-dots text-muted" style="font-size: 3rem;"></i>
        </div>
        <h6 class="text-muted mt-3">Chưa có tin nhắn nào</h6>
        <p class="text-muted small">Bắt đầu cuộc trò chuyện ngay bây giờ</p>
      </div>
    `;
  }

  setErrorState(title, message, retryCallback) {
    const chatMessagesEl = document.getElementById("chat-messages");
    chatMessagesEl.innerHTML = `
      <div class="text-center error-state p-4">
        <div class="error-icon mb-3">
          <i class="bi bi-exclamation-triangle text-danger" style="font-size: 2rem;"></i>
        </div>
        <h6 class="text-danger">${title}</h6>
        <p class="text-muted small">${message}</p>
        ${
          retryCallback
            ? `
          <button class="btn btn-sm btn-outline-primary mt-2 retry-button">
            <i class="bi bi-arrow-repeat me-1"></i>Thử lại
          </button>
        `
            : ""
        }
      </div>
    `;

    if (retryCallback) {
      const retryButton = chatMessagesEl.querySelector(".retry-button");
      if (retryButton) {
        retryButton.addEventListener("click", retryCallback);
      }
    }
  }

  clearMessages() {
    const chatMessagesEl = document.getElementById("chat-messages");
    chatMessagesEl.innerHTML = "";
  }

  scrollToBottom() {
    const chatMessagesEl = document.getElementById("chat-messages");
    chatMessagesEl.scrollTop = chatMessagesEl.scrollHeight;
  }
}

// Initialize the chat widget when the DOM is loaded
document.addEventListener("DOMContentLoaded", function () {
  // Add fallback for toastr if not defined
  if (typeof toastr === "undefined") {
    window.toastr = {
      success: (msg) => console.log("Success:", msg),
      error: (msg) => console.error("Error:", msg),
      warning: (msg) => console.warn("Warning:", msg),
      info: (msg) => console.info("Info:", msg),
    };
  }

  const chatWidgetInstance = new ChatWidget();
  window.chatWidget = chatWidgetInstance;
});
