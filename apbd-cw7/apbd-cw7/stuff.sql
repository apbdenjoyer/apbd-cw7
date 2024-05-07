-- Created by Vertabelo (http://vertabelo.com)
-- Last modification date: 2021-04-05 12:56:53.13

-- tables
-- Table: Order
CREATE TABLE "Order" (
                         IdOrder int  NOT NULL IDENTITY,
                         IdProduct int  NOT NULL,
                         Amount int  NOT NULL,
                         CreatedAt datetime  NOT NULL,
                         FulfilledAt datetime  NULL,
                         CONSTRAINT Order_pk PRIMARY KEY  (IdOrder)
);

-- Table: Product
CREATE TABLE Product (
                         IdProduct int  NOT NULL IDENTITY,
                         Name nvarchar(200)  NOT NULL,
                         Description nvarchar(200)  NOT NULL,
                         Price numeric(25,2)  NOT NULL,
                         CONSTRAINT Product_pk PRIMARY KEY  (IdProduct)
);

-- Table: Product_Warehouse
CREATE TABLE Product_Warehouse (
                                   IdProductWarehouse int  NOT NULL IDENTITY,
                                   IdWarehouse int  NOT NULL,
                                   IdProduct int  NOT NULL,
                                   IdOrder int  NOT NULL,
                                   Amount int  NOT NULL,
                                   Price numeric(25,2)  NOT NULL,
                                   CreatedAt datetime  NOT NULL,
                                   CONSTRAINT Product_Warehouse_pk PRIMARY KEY  (IdProductWarehouse)
);

-- Table: Warehouse
CREATE TABLE Warehouse (
                           IdWarehouse int  NOT NULL IDENTITY,
                           Name nvarchar(200)  NOT NULL,
                           Address nvarchar(200)  NOT NULL,
                           CONSTRAINT Warehouse_pk PRIMARY KEY  (IdWarehouse)
);

-- foreign keys
-- Reference: Product_Warehouse_Order (table: Product_Warehouse)
ALTER TABLE Product_Warehouse ADD CONSTRAINT Product_Warehouse_Order
    FOREIGN KEY (IdOrder)
        REFERENCES "Order" (IdOrder);

-- Reference: Receipt_Product (table: Order)
ALTER TABLE "Order" ADD CONSTRAINT Receipt_Product
    FOREIGN KEY (IdProduct)
        REFERENCES Product (IdProduct);

-- Reference: _Product (table: Product_Warehouse)
ALTER TABLE Product_Warehouse ADD CONSTRAINT _Product
    FOREIGN KEY (IdProduct)
        REFERENCES Product (IdProduct);

-- Reference: _Warehouse (table: Product_Warehouse)
ALTER TABLE Product_Warehouse ADD CONSTRAINT _Warehouse
    FOREIGN KEY (IdWarehouse)
        REFERENCES Warehouse (IdWarehouse);

-- End of file.

GO

INSERT INTO Warehouse(Name, Address)
VALUES('Warsaw', 'Kwiatowa 12');

GO

INSERT INTO Product(Name, Description, Price)
VALUES ('Abacavir', '', 25.5),
('Acyclovir', '', 45.0),
('Allopurinol', '', 30.8);

GO

INSERT INTO "Order"(IdProduct, Amount, CreatedAt)
VALUES((SELECT IdProduct FROM Product WHERE Name='Abacavir'), 125, GETDATE());

CREATE OR ALTER PROCEDURE AddProductToWarehouse
    @IdProduct INT,
    @IdWarehouse INT,
    @Amount INT,
    @CreatedAt DATETIME
    AS
BEGIN  
   
 DECLARE @IdOrder INT, 
         @Price DECIMAL(5,2) = 0, 
         @IdProductFromDb INT = 0;

SELECT TOP 1 @IdOrder = o.IdOrder
FROM [Order] o
    LEFT JOIN Product_Warehouse pw ON o.IdOrder = pw.IdOrder
WHERE o.IdProduct = @IdProduct
  AND o.Amount = @Amount
  AND pw.IdProductWarehouse IS NULL
  AND o.CreatedAt < @CreatedAt;

SELECT @IdProductFromDb = Product.IdProduct, @Price = Product.Price
FROM Product
WHERE IdProduct = @IdProduct;

IF @IdProductFromDb IS NULL
BEGIN  
  RAISERROR('Invalid parameter: Provided IdProduct does not exist', 18, 0);  
  RETURN;
END;  
  
 IF @IdOrder IS NULL
BEGIN  
  RAISERROR('Invalid parameter: There is no order to fulfill', 18, 0);  
  RETURN;
END;  
   
 IF NOT EXISTS(SELECT 1 FROM Warehouse WHERE IdWarehouse = @IdWarehouse)
BEGIN  
  RAISERROR('Invalid parameter: Provided IdWarehouse does not exist', 18, 0);  
  RETURN;
END;  
  
 SET XACT_ABORT ON;
BEGIN TRY
BEGIN TRAN;

UPDATE [Order]
SET FulfilledAt = @CreatedAt
WHERE IdOrder = @IdOrder;

INSERT INTO Product_Warehouse(IdWarehouse,
                              IdProduct,
                              IdOrder,
                              Amount,
                              Price,
                              CreatedAt)
VALUES(@IdWarehouse,
       @IdProduct,
       @IdOrder,
       @Amount,
       @Amount * @Price,
       @CreatedAt);

SELECT @@IDENTITY AS NewId;

COMMIT;
END TRY
BEGIN CATCH
IF @@TRANCOUNT > 0
BEGIN
ROLLBACK TRANSACTION;
END  
   
   -- Raise an error or log the exception, depending on your requirement.  
   DECLARE @ErrorMessage NVARCHAR(4000);  
   DECLARE @ErrorSeverity INT;  
   DECLARE @ErrorState INT;

SELECT @ErrorMessage = ERROR_MESSAGE(),
       @ErrorSeverity = ERROR_SEVERITY(),
       @ErrorState = ERROR_STATE();

RAISERROR(@ErrorMessage, @ErrorSeverity, @ErrorState);
END CATCH;

END
