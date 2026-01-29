# Hostlistic_Server
MyProject/
├── Domain/
│   ├── Entities/
│   │   └── Product.cs
│   ├── Interfaces/
│   │   └── IProductRepository.cs
│   └── ValueObjects/
│
├── Application/
│   ├── Interfaces/
│   │   └── IProductService.cs          ← IService ở đây
│   ├── Services/
│   │   └── ProductService.cs           ← Implementation
│   ├── DTOs/
│   │   └── ProductDto.cs
│   └── Mappers/
│
├── Infrastructure/
│   ├── Repositories/
│   │   └── ProductRepository.cs        ← Implement IProductRepository
│   └── Data/
│       └── AppDbContext.cs
│
└── API/ (hoặc Presentation)
    └── Controllers/
        └── ProductsController.cs
