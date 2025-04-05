-- Thêm mẫu CV cho người dùng Premium
INSERT INTO CVTemplates (TemplateId, Name, Description, HtmlContent, CssContent, ThumbnailPath, IsActive, CreatedAt)
VALUES 
-- Mẫu CV 1: Modern Clean
(
    NEWID(), 
    N'Modern Clean', 
    N'Mẫu CV hiện đại với thiết kế tối giản, phù hợp với hầu hết các ngành nghề. Thiết kế chuyên nghiệp và dễ đọc.',
    N'<div class="cv-modern-clean">
    <header class="cv-header">
        <div class="profile-info">
            <h1 class="name">{{Name}}</h1>
            <p class="job-title">{{JobTitle}}</p>
        </div>
        <div class="contact-info">
            <p><i class="fas fa-envelope"></i> {{Email}}</p>
            <p><i class="fas fa-phone"></i> {{Phone}}</p>
            <p><i class="fas fa-map-marker-alt"></i> {{Address}}</p>
        </div>
    </header>
    
    <main class="cv-content">
        <div class="left-column">
            <section class="cv-section">
                <h2 class="section-title">Học vấn</h2>
                <div class="section-content">
                    {{Education}}
                </div>
            </section>
            
            <section class="cv-section">
                <h2 class="section-title">Kỹ năng</h2>
                <div class="section-content">
                    {{Skills}}
                </div>
            </section>
            
            <section class="cv-section">
                <h2 class="section-title">Thông tin khác</h2>
                <div class="section-content">
                    {{Others}}
                </div>
            </section>
        </div>
        
        <div class="right-column">
            <section class="cv-section">
                <h2 class="section-title">Kinh nghiệm làm việc</h2>
                <div class="section-content">
                    {{Experience}}
                </div>
            </section>
        </div>
    </main>
</div>',
    N'/* Modern Clean CV Style */
.cv-modern-clean {
    font-family: "Segoe UI", Arial, sans-serif;
    color: #333;
    max-width: 21cm;
    margin: 0 auto;
    padding: 2cm;
    background-color: #fff;
}

.cv-header {
    display: flex;
    justify-content: space-between;
    align-items: flex-start;
    margin-bottom: 2rem;
    padding-bottom: 1rem;
    border-bottom: 2px solid #3498db;
}

.name {
    font-size: 2.5rem;
    font-weight: 700;
    color: #2c3e50;
    margin: 0;
    line-height: 1.2;
}

.job-title {
    font-size: 1.3rem;
    color: #3498db;
    margin: 0.5rem 0 0;
}

.contact-info {
    text-align: right;
}

.contact-info p {
    margin: 0.3rem 0;
    font-size: 0.95rem;
}

.contact-info i {
    color: #3498db;
    width: 20px;
    margin-right: 5px;
}

.cv-content {
    display: flex;
    gap: 2rem;
}

.left-column {
    flex: 4;
}

.right-column {
    flex: 6;
}

.cv-section {
    margin-bottom: 1.5rem;
}

.section-title {
    font-size: 1.2rem;
    font-weight: 600;
    color: #3498db;
    margin-bottom: 0.8rem;
    padding-bottom: 0.3rem;
    border-bottom: 1px solid #e0e0e0;
}

.section-content {
    font-size: 0.95rem;
    line-height: 1.5;
}',
    N'/images/cv-templates/modern-clean.jpg',
    1,
    GETDATE()
),

-- Mẫu CV 2: Creative Professional
(
    NEWID(), 
    N'Creative Professional', 
    N'Mẫu CV sáng tạo với thiết kế độc đáo, phù hợp với các ngành sáng tạo như thiết kế, marketing, nghệ thuật.',
    N'<div class="cv-creative">
    <div class="cv-header">
        <div class="profile-section">
            <h1 class="name">{{Name}}</h1>
            <p class="job-title">{{JobTitle}}</p>
            <div class="contact-info">
                <p><i class="fas fa-envelope"></i> {{Email}}</p>
                <p><i class="fas fa-phone"></i> {{Phone}}</p>
                <p><i class="fas fa-map-marker-alt"></i> {{Address}}</p>
            </div>
        </div>
    </div>
    
    <div class="cv-body">
        <div class="main-content">
            <section class="cv-section">
                <h2 class="section-title"><span class="title-icon"><i class="fas fa-briefcase"></i></span>Kinh nghiệm làm việc</h2>
                <div class="section-content">
                    {{Experience}}
                </div>
            </section>
            
            <section class="cv-section">
                <h2 class="section-title"><span class="title-icon"><i class="fas fa-graduation-cap"></i></span>Học vấn</h2>
                <div class="section-content">
                    {{Education}}
                </div>
            </section>
        </div>
        
        <div class="side-content">
            <section class="cv-section">
                <h2 class="section-title"><span class="title-icon"><i class="fas fa-cogs"></i></span>Kỹ năng</h2>
                <div class="section-content">
                    {{Skills}}
                </div>
            </section>
            
            <section class="cv-section">
                <h2 class="section-title"><span class="title-icon"><i class="fas fa-info-circle"></i></span>Thông tin khác</h2>
                <div class="section-content">
                    {{Others}}
                </div>
            </section>
        </div>
    </div>
</div>',
    N'/* Creative Professional CV Style */
.cv-creative {
    font-family: "Montserrat", "Segoe UI", Arial, sans-serif;
    color: #333;
    max-width: 21cm;
    margin: 0 auto;
    background-color: #fff;
    position: relative;
}

.cv-header {
    background: linear-gradient(135deg, #6a11cb 0%, #2575fc 100%);
    color: white;
    padding: 2.5rem 2rem;
    border-radius: 0 0 30% 0;
}

.name {
    font-size: 2.8rem;
    font-weight: 700;
    margin: 0;
    letter-spacing: -1px;
    text-shadow: 2px 2px 4px rgba(0,0,0,0.2);
}

.job-title {
    font-size: 1.4rem;
    font-weight: 300;
    margin: 0.5rem 0 1.5rem;
    opacity: 0.9;
}

.contact-info {
    display: flex;
    flex-wrap: wrap;
    gap: 1rem;
}

.contact-info p {
    margin: 0;
    font-size: 0.9rem;
    display: flex;
    align-items: center;
}

.contact-info i {
    margin-right: 0.5rem;
}

.cv-body {
    display: flex;
    padding: 2rem;
}

.main-content {
    flex: 7;
    padding-right: 2rem;
}

.side-content {
    flex: 3;
    background-color: #f8f9fa;
    padding: 1.5rem;
    border-radius: 10px;
}

.cv-section {
    margin-bottom: 2rem;
}

.section-title {
    font-size: 1.3rem;
    font-weight: 600;
    color: #2575fc;
    margin-bottom: 1rem;
    display: flex;
    align-items: center;
    border-bottom: 2px solid #e0e0e0;
    padding-bottom: 0.5rem;
}

.title-icon {
    display: inline-flex;
    align-items: center;
    justify-content: center;
    width: 30px;
    height: 30px;
    background-color: #2575fc;
    color: white;
    border-radius: 50%;
    margin-right: 0.8rem;
}

.section-content {
    font-size: 0.95rem;
    line-height: 1.6;
}',
    N'/images/cv-templates/creative-professional.jpg',
    1,
    GETDATE()
),

-- Mẫu CV 3: Minimal Tech
(
    NEWID(), 
    N'Minimal Tech', 
    N'Mẫu CV tối giản dành cho các vị trí công nghệ như lập trình viên, kỹ sư phần mềm, chuyên gia IT.',
    N'<div class="cv-minimal-tech">
    <header class="cv-header">
        <div class="header-content">
            <h1 class="name">{{Name}}</h1>
            <p class="job-title">{{JobTitle}}</p>
        </div>
        <div class="contact-bar">
            <span><i class="fas fa-envelope"></i> {{Email}}</span>
            <span><i class="fas fa-phone"></i> {{Phone}}</span>
            <span><i class="fas fa-map-marker-alt"></i> {{Address}}</span>
        </div>
    </header>
    
    <main class="cv-content">
        <div class="content-grid">
            <section class="cv-section skills-section">
                <h2 class="section-title">Kỹ năng</h2>
                <div class="section-content">
                    {{Skills}}
                </div>
            </section>
            
            <section class="cv-section experience-section">
                <h2 class="section-title">Kinh nghiệm làm việc</h2>
                <div class="section-content">
                    {{Experience}}
                </div>
            </section>
            
            <section class="cv-section education-section">
                <h2 class="section-title">Học vấn</h2>
                <div class="section-content">
                    {{Education}}
                </div>
            </section>
            
            <section class="cv-section others-section">
                <h2 class="section-title">Thông tin khác</h2>
                <div class="section-content">
                    {{Others}}
                </div>
            </section>
        </div>
    </main>
</div>',
    N'/* Minimal Tech CV Style */
.cv-minimal-tech {
    font-family: "Roboto", "Segoe UI", Arial, sans-serif;
    color: #2c3e50;
    max-width: 21cm;
    margin: 0 auto;
    background-color: #fff;
    padding: 0;
}

.cv-header {
    background-color: #2c3e50;
    color: white;
    padding: 2rem;
}

.header-content {
    text-align: center;
    margin-bottom: 1.5rem;
}

.name {
    font-size: 2.5rem;
    font-weight: 300;
    margin: 0;
    letter-spacing: 2px;
}

.job-title {
    font-size: 1.2rem;
    font-weight: 400;
    margin: 0.5rem 0 0;
    color: #3498db;
}

.contact-bar {
    display: flex;
    justify-content: center;
    flex-wrap: wrap;
    gap: 1.5rem;
    font-size: 0.9rem;
}

.contact-bar span {
    display: flex;
    align-items: center;
}

.contact-bar i {
    margin-right: 0.5rem;
    color: #3498db;
}

.cv-content {
    padding: 2rem;
}

.content-grid {
    display: grid;
    grid-template-columns: 1fr 1fr;
    gap: 2rem;
}

.skills-section {
    grid-column: 1;
    grid-row: 1;
}

.experience-section {
    grid-column: 2;
    grid-row: 1 / span 2;
}

.education-section {
    grid-column: 1;
    grid-row: 2;
}

.others-section {
    grid-column: 1 / span 2;
    grid-row: 3;
}

.section-title {
    font-size: 1.2rem;
    font-weight: 500;
    color: #2c3e50;
    margin-bottom: 1rem;
    padding-bottom: 0.5rem;
    border-bottom: 2px solid #3498db;
}

.section-content {
    font-size: 0.95rem;
    line-height: 1.6;
}',
    N'/images/cv-templates/minimal-tech.jpg',
    1,
    GETDATE()
),

-- Mẫu CV 4: Business Professional
(
    NEWID(), 
    N'Business Professional', 
    N'Mẫu CV chuyên nghiệp dành cho các vị trí quản lý, kinh doanh, tài chính và các ngành nghề truyền thống.',
    N'<div class="cv-business">
    <header class="cv-header">
        <h1 class="name">{{Name}}</h1>
        <p class="job-title">{{JobTitle}}</p>
        <div class="contact-info">
            <div class="contact-item">
                <i class="fas fa-envelope"></i>
                <span>{{Email}}</span>
            </div>
            <div class="contact-item">
                <i class="fas fa-phone"></i>
                <span>{{Phone}}</span>
            </div>
            <div class="contact-item">
                <i class="fas fa-map-marker-alt"></i>
                <span>{{Address}}</span>
            </div>
        </div>
    </header>
    
    <main class="cv-content">
        <section class="cv-section">
            <h2 class="section-title">Kinh nghiệm làm việc</h2>
            <div class="section-content">
                {{Experience}}
            </div>
        </section>
        
        <section class="cv-section">
            <h2 class="section-title">Học vấn</h2>
            <div class="section-content">
                {{Education}}
            </div>
        </section>
        
        <section class="cv-section">
            <h2 class="section-title">Kỹ năng</h2>
            <div class="section-content">
                {{Skills}}
            </div>
        </section>
        
        <section class="cv-section">
            <h2 class="section-title">Thông tin khác</h2>
            <div class="section-content">
                {{Others}}
            </div>
        </section>
    </main>
</div>',
    N'/* Business Professional CV Style */
.cv-business {
    font-family: "Times New Roman", serif;
    color: #333;
    max-width: 21cm;
    margin: 0 auto;
    padding: 2cm;
    background-color: #fff;
    border: 1px solid #ddd;
}

.cv-header {
    text-align: center;
    margin-bottom: 2rem;
    padding-bottom: 1.5rem;
    border-bottom: 2px solid #333;
}

.name {
    font-size: 2.2rem;
    font-weight: bold;
    margin: 0;
    text-transform: uppercase;
    letter-spacing: 1px;
}

.job-title {
    font-size: 1.2rem;
    margin: 0.5rem 0 1rem;
    font-style: italic;
}

.contact-info {
    display: flex;
    justify-content: center;
    flex-wrap: wrap;
    gap: 1.5rem;
}

.contact-item {
    display: flex;
    align-items: center;
    font-size: 0.9rem;
}

.contact-item i {
    margin-right: 0.5rem;
}

.cv-section {
    margin-bottom: 1.8rem;
}

.section-title {
    font-size: 1.3rem;
    font-weight: bold;
    margin-bottom: 1rem;
    text-transform: uppercase;
    letter-spacing: 1px;
    border-bottom: 1px solid #ddd;
    padding-bottom: 0.3rem;
}

.section-content {
    font-size: 1rem;
    line-height: 1.5;
}',
    N'/images/cv-templates/business-professional.jpg',
    1,
    GETDATE()
);
