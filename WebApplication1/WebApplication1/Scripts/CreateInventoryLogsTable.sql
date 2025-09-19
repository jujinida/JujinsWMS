CREATE TABLE InventoryLogs (
    log_id INT IDENTITY(1,1) PRIMARY KEY,
    product_id INT NOT NULL,
    change_type NVARCHAR(20) NOT NULL, -- '입고', '출고' 등
    quantity_changed INT NOT NULL, -- 변경된 수량
    current_quantity INT NOT NULL, -- 변경 후 현재 재고 수량
    created_at DATETIME DEFAULT GETDATE(),
    FOREIGN KEY (product_id) REFERENCES Products(product_id)
);
