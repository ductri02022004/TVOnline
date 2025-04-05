-- Script để thêm mẫu CV chuyên nghiệp vào cơ sở dữ liệu
INSERT INTO CVTemplates (TemplateId, Name, Description, IsActive, ThumbnailPath, HtmlContent, CssContent, CreatedAt)
VALUES (
    NEWID(), -- Tạo ID mới
    N'CV Chuyên Nghiệp', -- Tên mẫu
    N'Mẫu CV chuyên nghiệp với thiết kế hiện đại, phù hợp cho các vị trí công việc chuyên nghiệp trong nhiều lĩnh vực.', -- Mô tả
    1, -- Trạng thái hoạt động
    '/images/cv-templates/professional-cv-template.jpg', -- Đường dẫn hình ảnh
    -- Nội dung HTML
    N'<!DOCTYPE html>
<html lang="vi">
<head>
    <meta charset="UTF-8">
    <meta name="viewport" content="width=device-width, initial-scale=1.0">
    <title>CV Chuyên Nghiệp</title>
</head>
<body>
    <div class="cv-container">
        <!-- Header -->
        <header class="cv-header">
            <div class="profile-info">
                <h1 class="name">{{Name}}</h1>
                <h2 class="job-title">{{JobTitle}}</h2>
                <div class="summary">
                    <p>{{Summary}}</p>
                </div>
            </div>
            <div class="contact-info">
                <div class="contact-item">
                    <i class="bi bi-envelope"></i>
                    <span>{{Email}}</span>
                </div>
                <div class="contact-item">
                    <i class="bi bi-telephone"></i>
                    <span>{{Phone}}</span>
                </div>
                <div class="contact-item">
                    <i class="bi bi-geo-alt"></i>
                    <span>{{Address}}</span>
                </div>
            </div>
        </header>

        <!-- Main Content -->
        <div class="cv-content">
            <!-- Left Column -->
            <div class="left-column">
                <section class="cv-section">
                    <h3 class="section-title">Học vấn</h3>
                    <div class="section-content education-content">
                        {{Education}}
                    </div>
                </section>

                <section class="cv-section">
                    <h3 class="section-title">Kỹ năng</h3>
                    <div class="section-content skills-content">
                        {{Skills}}
                    </div>
                </section>

                <section class="cv-section">
                    <h3 class="section-title">Ngôn ngữ</h3>
                    <div class="section-content languages-content">
                        {{Languages}}
                    </div>
                </section>

                <section class="cv-section">
                    <h3 class="section-title">Sở thích</h3>
                    <div class="section-content interests-content">
                        {{Interests}}
                    </div>
                </section>
            </div>

            <!-- Right Column -->
            <div class="right-column">
                <section class="cv-section">
                    <h3 class="section-title">Kinh nghiệm làm việc</h3>
                    <div class="section-content experience-content">
                        {{Experience}}
                    </div>
                </section>

                <section class="cv-section">
                    <h3 class="section-title">Dự án</h3>
                    <div class="section-content projects-content">
                        {{Projects}}
                    </div>
                </section>

                <section class="cv-section">
                    <h3 class="section-title">Chứng chỉ</h3>
                    <div class="section-content certificates-content">
                        {{Certificates}}
                    </div>
                </section>
            </div>
        </div>
    </div>
</body>
</html>',
    -- Nội dung CSS
    N'/* Professional CV Template CSS */
@import url(''https://fonts.googleapis.com/css2?family=Roboto:wght@300;400;500;700&display=swap'');

* {
    margin: 0;
    padding: 0;
    box-sizing: border-box;
}

body {
    font-family: ''Roboto'', sans-serif;
    color: #333;
    line-height: 1.6;
    background-color: #f5f5f5;
}

.cv-container {
    max-width: 210mm;
    margin: 0 auto;
    background-color: #fff;
    box-shadow: 0 0 10px rgba(0, 0, 0, 0.1);
    position: relative;
}

/* Header */
.cv-header {
    display: flex;
    padding: 40px;
    background-color: #2c3e50;
    color: #fff;
}

.profile-info {
    flex: 2;
    padding-right: 30px;
}

.name {
    font-size: 36px;
    font-weight: 700;
    margin-bottom: 5px;
    color: #ecf0f1;
}

.job-title {
    font-size: 20px;
    font-weight: 400;
    color: #3498db;
    margin-bottom: 15px;
}

.summary {
    font-size: 14px;
    line-height: 1.6;
}

.contact-info {
    flex: 1;
    border-left: 1px solid rgba(255, 255, 255, 0.2);
    padding-left: 30px;
    display: flex;
    flex-direction: column;
    justify-content: center;
}

.contact-item {
    display: flex;
    align-items: center;
    margin-bottom: 10px;
}

.contact-item i {
    margin-right: 10px;
    color: #3498db;
}

/* Main Content */
.cv-content {
    display: flex;
    padding: 40px;
}

.left-column {
    flex: 1;
    padding-right: 30px;
}

.right-column {
    flex: 2;
    border-left: 1px solid #e0e0e0;
    padding-left: 30px;
}

/* Sections */
.cv-section {
    margin-bottom: 30px;
}

.section-title {
    font-size: 18px;
    font-weight: 500;
    color: #2c3e50;
    margin-bottom: 15px;
    padding-bottom: 5px;
    border-bottom: 2px solid #3498db;
}

.section-content {
    font-size: 14px;
}

/* Education */
.education-item {
    margin-bottom: 15px;
}

.education-item h4 {
    font-weight: 500;
    margin-bottom: 5px;
}

.education-item p {
    color: #666;
    font-size: 13px;
}

/* Experience */
.experience-item {
    margin-bottom: 20px;
}

.experience-item h4 {
    font-weight: 500;
    margin-bottom: 5px;
}

.experience-item p {
    color: #666;
    font-size: 13px;
    margin-bottom: 8px;
}

.experience-item ul {
    padding-left: 20px;
}

.experience-item li {
    margin-bottom: 5px;
}

/* Skills */
.skills-content ul {
    list-style-type: none;
    display: flex;
    flex-wrap: wrap;
}

.skills-content li {
    background-color: #f0f8ff;
    padding: 5px 10px;
    border-radius: 3px;
    margin-right: 10px;
    margin-bottom: 10px;
    font-size: 13px;
}

/* Projects */
.project-item {
    margin-bottom: 15px;
}

.project-item h4 {
    font-weight: 500;
    margin-bottom: 5px;
}

.project-item p {
    color: #666;
    font-size: 13px;
}

/* Print Styles */
@media print {
    body {
        background-color: #fff;
    }
    
    .cv-container {
        box-shadow: none;
        max-width: 100%;
    }
}',
    GETDATE() -- Ngày tạo
);
