-- Products 테이블 생성 스크립트
-- Azure SQL Database에서 실행

USE jujinscshop;
GO

-- 기존 테이블이 있다면 삭제 (주의: 데이터가 모두 삭제됩니다)
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Products]') AND type in (N'U'))
BEGIN
    DROP TABLE [dbo].[Products];
    PRINT '기존 Products 테이블이 삭제되었습니다.';
END
GO

-- Products 테이블 생성
CREATE TABLE Products (
    product_id INT IDENTITY(1,1) PRIMARY KEY, -- 자동 증가 기본키
    product_name NVARCHAR(100) NOT NULL, -- 상품명
    category NVARCHAR(50), -- 상품 분류
    price DECIMAL(18, 2) NOT NULL, -- 상품 가격
    stock_quantity INT NOT NULL CHECK (stock_quantity >= 0), -- 재고 수량, 0 이상이어야 함
    pd_url VARCHAR(500) DEFAULT NULL, -- 제품 이미지 주소
    created_at DATETIME2 DEFAULT GETDATE(), -- 생성일시
    updated_at DATETIME2 DEFAULT GETDATE() -- 수정일시
);
GO

-- 인덱스 생성 (성능 최적화)
CREATE INDEX IX_Products_Category ON Products(category);
CREATE INDEX IX_Products_ProductName ON Products(product_name);
GO

-- 테이블 생성 완료 메시지
PRINT 'Products 테이블이 성공적으로 생성되었습니다.';
PRINT '컬럼 정보:';
PRINT '- product_id: 자동 증가 기본키';
PRINT '- product_name: 상품명 (필수)';
PRINT '- category: 상품 분류';
PRINT '- price: 상품 가격 (필수)';
PRINT '- stock_quantity: 재고 수량 (필수, 0 이상)';
PRINT '- pd_url: 제품 이미지 URL';
PRINT '- created_at: 생성일시';
PRINT '- updated_at: 수정일시';
GO
