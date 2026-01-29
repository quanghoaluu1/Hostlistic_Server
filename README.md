# Hostlistic_Server

```text
MyProject/
├── Domain/                         # Chứa logic nghiệp vụ lõi (Core Business Logic)
│   ├── Entities/                   # Các đối tượng nghiệp vụ (VD: Product.cs)
│   ├── Interfaces/                 # Các bản thiết kế cho Repository (Abstractions)
│   │   └── IProductRepository.cs
│   └── ValueObjects/               # Các đối tượng định nghĩa bằng giá trị
│
├── Application/                    # Chứa Use Cases và logic ứng dụng
│   ├── Interfaces/                 # Định nghĩa các Service cho tầng API
│   │   └── IProductService.cs
│   ├── Services/                   # Triển khai (Implementation) của IProductService
│   │   └── ProductService.cs
│   ├── DTOs/                       # Đối tượng vận chuyển dữ liệu (Data Transfer Objects)
│   │   └── ProductDto.cs
│   └── Mappers/                    # Cấu hình chuyển đổi giữa Entity <-> DTO
│
├── Infrastructure/                 # Các công nghệ bên ngoài (Persistence, Identity)
│   ├── Repositories/               # Triển khai thực tế các interface từ tầng Domain
│   │   └── ProductRepository.cs
│   └── Data/                       # Cấu hình Database (Entity Framework Core)
│       └── AppDbContext.cs
│
└── API/ (hoặc Presentation)        # Điểm đầu vào của ứng dụng (Entry Point)
    └── Controllers/                # Nhận request, gọi Service và trả về kết quả
        └── ProductsController.cs
