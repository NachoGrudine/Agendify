-- =============================================
-- Script de datos de prueba para Agendify
-- Business ID: 1
-- Provider ID: 1
-- Fecha inicio: 6 de febrero de 2026
-- =============================================

USE [AgendifyDb];
GO

-- =============================================
-- 1. SERVICIOS (10 servicios)
-- =============================================
SET IDENTITY_INSERT [Services] ON;

INSERT INTO [Services] ([Id], [BusinessId], [Name], [Description], [DurationMinutes], [Price])
VALUES 
    (1, 1, 'Corte de Cabello', 'Corte clásico de cabello', 30, 5000),
    (2, 1, 'Coloración', 'Coloración completa de cabello', 90, 12000),
    (3, 1, 'Manicura', 'Manicura con esmaltado tradicional', 45, 4000),
    (4, 1, 'Pedicura', 'Pedicura completa con masaje', 60, 5500),
    (5, 1, 'Depilación Facial', 'Depilación de cejas y bozo', 20, 3000),
    (6, 1, 'Tratamiento Capilar', 'Hidratación profunda', 45, 8000),
    (7, 1, 'Maquillaje Social', 'Maquillaje para eventos', 60, 10000),
    (8, 1, 'Diseño de Cejas', 'Perfilado y diseño de cejas', 30, 3500),
    (9, 1, 'Alisado Permanente', 'Tratamiento de alisado', 120, 18000),
    (10, 1, 'Masaje Capilar', 'Masaje relajante del cuero cabelludo', 30, 4500);

SET IDENTITY_INSERT [Services] OFF;
GO

-- =============================================
-- 2. CLIENTES (15 clientes)
-- =============================================
SET IDENTITY_INSERT [Customers] ON;

INSERT INTO [Customers] ([Id], [BusinessId], [Name], [Phone], [Email])
VALUES 
    (1, 1, 'María González', '1134567890', 'maria.gonzalez@email.com'),
    (2, 1, 'Laura Martínez', '1145678901', 'laura.martinez@email.com'),
    (3, 1, 'Ana Rodríguez', '1156789012', 'ana.rodriguez@email.com'),
    (4, 1, 'Sofía López', '1167890123', 'sofia.lopez@email.com'),
    (5, 1, 'Valentina Pérez', '1178901234', 'valentina.perez@email.com'),
    (6, 1, 'Camila García', '1189012345', 'camila.garcia@email.com'),
    (7, 1, 'Isabella Fernández', '1190123456', 'isabella.fernandez@email.com'),
    (8, 1, 'Martina Díaz', '1101234567', 'martina.diaz@email.com'),
    (9, 1, 'Lucía Sánchez', '1112345678', 'lucia.sanchez@email.com'),
    (10, 1, 'Emma Ramírez', '1123456789', 'emma.ramirez@email.com'),
    (11, 1, 'Mía Torres', '1134567891', 'mia.torres@email.com'),
    (12, 1, 'Victoria Flores', '1145678902', 'victoria.flores@email.com'),
    (13, 1, 'Catalina Ruiz', '1156789013', 'catalina.ruiz@email.com'),
    (14, 1, 'Julieta Morales', '1167890124', 'julieta.morales@email.com'),
    (15, 1, 'Renata Castro', '1178901235', 'renata.castro@email.com');

SET IDENTITY_INSERT [Customers] OFF;
GO

-- =============================================
-- 3. TURNOS (Appointments)
-- Distribución: 6-28 de febrero de 2026
-- Varios turnos por día con diferentes servicios y clientes
-- =============================================
SET IDENTITY_INSERT [Appointments] ON;

-- JUEVES 6 de Febrero 2026
INSERT INTO [Appointments] ([Id], [BusinessId], [ProviderId], [CustomerId], [ServiceId], [StartTime], [EndTime], [Status], [Notes])
VALUES 
    (1, 1, 1, 1, 1, '2026-02-06 09:00:00', '2026-02-06 09:30:00', 'Scheduled', 'Primera vez'),
    (2, 1, 1, 2, 3, '2026-02-06 10:00:00', '2026-02-06 10:45:00', 'Scheduled', NULL),
    (3, 1, 1, 3, 5, '2026-02-06 11:00:00', '2026-02-06 11:20:00', 'Scheduled', NULL),
    (4, 1, 1, 4, 2, '2026-02-06 14:00:00', '2026-02-06 15:30:00', 'Scheduled', 'Color rubio'),
    (5, 1, 1, 5, 8, '2026-02-06 16:00:00', '2026-02-06 16:30:00', 'Scheduled', NULL);

-- VIERNES 7 de Febrero 2026
INSERT INTO [Appointments] ([Id], [BusinessId], [ProviderId], [CustomerId], [ServiceId], [StartTime], [EndTime], [Status], [Notes])
VALUES 
    (6, 1, 1, 6, 1, '2026-02-07 09:00:00', '2026-02-07 09:30:00', 'Scheduled', NULL),
    (7, 1, 1, 7, 4, '2026-02-07 10:00:00', '2026-02-07 11:00:00', 'Scheduled', 'Cliente VIP'),
    (8, 1, 1, 8, 6, '2026-02-07 11:30:00', '2026-02-07 12:15:00', 'Scheduled', NULL),
    (9, 1, 1, 9, 1, '2026-02-07 14:00:00', '2026-02-07 14:30:00', 'Scheduled', NULL),
    (10, 1, 1, 10, 7, '2026-02-07 15:00:00', '2026-02-07 16:00:00', 'Scheduled', 'Evento de noche');

-- SÁBADO 8 de Febrero 2026
INSERT INTO [Appointments] ([Id], [BusinessId], [ProviderId], [CustomerId], [ServiceId], [StartTime], [EndTime], [Status], [Notes])
VALUES 
    (11, 1, 1, 11, 1, '2026-02-08 09:00:00', '2026-02-08 09:30:00', 'Scheduled', NULL),
    (12, 1, 1, 12, 3, '2026-02-08 09:45:00', '2026-02-08 10:30:00', 'Scheduled', NULL),
    (13, 1, 1, 13, 9, '2026-02-08 11:00:00', '2026-02-08 13:00:00', 'Scheduled', 'Alisado brasilero'),
    (14, 1, 1, 14, 1, '2026-02-08 14:00:00', '2026-02-08 14:30:00', 'Scheduled', NULL),
    (15, 1, 1, 15, 4, '2026-02-08 15:00:00', '2026-02-08 16:00:00', 'Scheduled', NULL),
    (16, 1, 1, 1, 10, '2026-02-08 16:30:00', '2026-02-08 17:00:00', 'Scheduled', NULL);

-- LUNES 10 de Febrero 2026
INSERT INTO [Appointments] ([Id], [BusinessId], [ProviderId], [CustomerId], [ServiceId], [StartTime], [EndTime], [Status], [Notes])
VALUES 
    (17, 1, 1, 2, 1, '2026-02-10 09:00:00', '2026-02-10 09:30:00', 'Scheduled', NULL),
    (18, 1, 1, 3, 8, '2026-02-10 10:00:00', '2026-02-10 10:30:00', 'Scheduled', NULL),
    (19, 1, 1, 4, 3, '2026-02-10 11:00:00', '2026-02-10 11:45:00', 'Scheduled', NULL),
    (20, 1, 1, 5, 6, '2026-02-10 14:00:00', '2026-02-10 14:45:00', 'Scheduled', 'Tratamiento de keratina'),
    (21, 1, 1, 6, 1, '2026-02-10 15:30:00', '2026-02-10 16:00:00', 'Scheduled', NULL);

-- MARTES 11 de Febrero 2026
INSERT INTO [Appointments] ([Id], [BusinessId], [ProviderId], [CustomerId], [ServiceId], [StartTime], [EndTime], [Status], [Notes])
VALUES 
    (22, 1, 1, 7, 2, '2026-02-11 09:00:00', '2026-02-11 10:30:00', 'Scheduled', 'Mechas balayage'),
    (23, 1, 1, 8, 1, '2026-02-11 11:00:00', '2026-02-11 11:30:00', 'Scheduled', NULL),
    (24, 1, 1, 9, 5, '2026-02-11 12:00:00', '2026-02-11 12:20:00', 'Scheduled', NULL),
    (25, 1, 1, 10, 4, '2026-02-11 14:00:00', '2026-02-11 15:00:00', 'Scheduled', NULL),
    (26, 1, 1, 11, 7, '2026-02-11 15:30:00', '2026-02-11 16:30:00', 'Scheduled', 'Casamiento');

-- MIÉRCOLES 12 de Febrero 2026
INSERT INTO [Appointments] ([Id], [BusinessId], [ProviderId], [CustomerId], [ServiceId], [StartTime], [EndTime], [Status], [Notes])
VALUES 
    (27, 1, 1, 12, 1, '2026-02-12 09:00:00', '2026-02-12 09:30:00', 'Scheduled', NULL),
    (28, 1, 1, 13, 3, '2026-02-12 10:00:00', '2026-02-12 10:45:00', 'Scheduled', 'Diseño francés'),
    (29, 1, 1, 14, 8, '2026-02-12 11:30:00', '2026-02-12 12:00:00', 'Scheduled', NULL),
    (30, 1, 1, 15, 6, '2026-02-12 14:00:00', '2026-02-12 14:45:00', 'Scheduled', NULL),
    (31, 1, 1, 1, 1, '2026-02-12 15:30:00', '2026-02-12 16:00:00', 'Scheduled', 'Cliente regular');

-- JUEVES 13 de Febrero 2026
INSERT INTO [Appointments] ([Id], [BusinessId], [ProviderId], [CustomerId], [ServiceId], [StartTime], [EndTime], [Status], [Notes])
VALUES 
    (32, 1, 1, 2, 9, '2026-02-13 09:00:00', '2026-02-13 11:00:00', 'Scheduled', 'Alisado japonés'),
    (33, 1, 1, 3, 1, '2026-02-13 11:30:00', '2026-02-13 12:00:00', 'Scheduled', NULL),
    (34, 1, 1, 4, 4, '2026-02-13 14:00:00', '2026-02-13 15:00:00', 'Scheduled', NULL),
    (35, 1, 1, 5, 10, '2026-02-13 15:30:00', '2026-02-13 16:00:00', 'Scheduled', 'Muy estresada'),
    (36, 1, 1, 6, 5, '2026-02-13 16:30:00', '2026-02-13 16:50:00', 'Scheduled', NULL);

-- VIERNES 14 de Febrero 2026 (San Valentín - día ocupado)
INSERT INTO [Appointments] ([Id], [BusinessId], [ProviderId], [CustomerId], [ServiceId], [StartTime], [EndTime], [Status], [Notes])
VALUES 
    (37, 1, 1, 7, 7, '2026-02-14 09:00:00', '2026-02-14 10:00:00', 'Scheduled', 'Cena romántica'),
    (38, 1, 1, 8, 1, '2026-02-14 10:30:00', '2026-02-14 11:00:00', 'Scheduled', NULL),
    (39, 1, 1, 9, 3, '2026-02-14 11:30:00', '2026-02-14 12:15:00', 'Scheduled', 'Uñas rojas'),
    (40, 1, 1, 10, 8, '2026-02-14 13:00:00', '2026-02-14 13:30:00', 'Scheduled', NULL),
    (41, 1, 1, 11, 7, '2026-02-14 14:00:00', '2026-02-14 15:00:00', 'Scheduled', 'Fiesta de noche'),
    (42, 1, 1, 12, 1, '2026-02-14 15:30:00', '2026-02-14 16:00:00', 'Scheduled', NULL),
    (43, 1, 1, 13, 4, '2026-02-14 16:30:00', '2026-02-14 17:30:00', 'Scheduled', NULL);

-- SÁBADO 15 de Febrero 2026
INSERT INTO [Appointments] ([Id], [BusinessId], [ProviderId], [CustomerId], [ServiceId], [StartTime], [EndTime], [Status], [Notes])
VALUES 
    (44, 1, 1, 14, 2, '2026-02-15 09:00:00', '2026-02-15 10:30:00', 'Scheduled', 'Cambio de look'),
    (45, 1, 1, 15, 1, '2026-02-15 11:00:00', '2026-02-15 11:30:00', 'Scheduled', NULL),
    (46, 1, 1, 1, 6, '2026-02-15 12:00:00', '2026-02-15 12:45:00', 'Scheduled', NULL),
    (47, 1, 1, 2, 3, '2026-02-15 14:00:00', '2026-02-15 14:45:00', 'Scheduled', NULL),
    (48, 1, 1, 3, 10, '2026-02-15 15:00:00', '2026-02-15 15:30:00', 'Scheduled', NULL),
    (49, 1, 1, 4, 5, '2026-02-15 16:00:00', '2026-02-15 16:20:00', 'Scheduled', NULL);

-- LUNES 17 de Febrero 2026
INSERT INTO [Appointments] ([Id], [BusinessId], [ProviderId], [CustomerId], [ServiceId], [StartTime], [EndTime], [Status], [Notes])
VALUES 
    (50, 1, 1, 5, 1, '2026-02-17 09:00:00', '2026-02-17 09:30:00', 'Scheduled', NULL),
    (51, 1, 1, 6, 8, '2026-02-17 10:00:00', '2026-02-17 10:30:00', 'Scheduled', NULL),
    (52, 1, 1, 7, 3, '2026-02-17 11:00:00', '2026-02-17 11:45:00', 'Scheduled', NULL),
    (53, 1, 1, 8, 4, '2026-02-17 14:00:00', '2026-02-17 15:00:00', 'Scheduled', NULL),
    (54, 1, 1, 9, 1, '2026-02-17 15:30:00', '2026-02-17 16:00:00', 'Scheduled', NULL);

-- MARTES 18 de Febrero 2026
INSERT INTO [Appointments] ([Id], [BusinessId], [ProviderId], [CustomerId], [ServiceId], [StartTime], [EndTime], [Status], [Notes])
VALUES 
    (55, 1, 1, 10, 2, '2026-02-18 09:00:00', '2026-02-18 10:30:00', 'Scheduled', 'Cobertura de canas'),
    (56, 1, 1, 11, 1, '2026-02-18 11:00:00', '2026-02-18 11:30:00', 'Scheduled', NULL),
    (57, 1, 1, 12, 6, '2026-02-18 12:00:00', '2026-02-18 12:45:00', 'Scheduled', 'Cabello dañado'),
    (58, 1, 1, 13, 5, '2026-02-18 14:00:00', '2026-02-18 14:20:00', 'Scheduled', NULL),
    (59, 1, 1, 14, 7, '2026-02-18 15:00:00', '2026-02-18 16:00:00', 'Scheduled', 'Reunión importante');

-- MIÉRCOLES 19 de Febrero 2026
INSERT INTO [Appointments] ([Id], [BusinessId], [ProviderId], [CustomerId], [ServiceId], [StartTime], [EndTime], [Status], [Notes])
VALUES 
    (60, 1, 1, 15, 1, '2026-02-19 09:00:00', '2026-02-19 09:30:00', 'Scheduled', NULL),
    (61, 1, 1, 1, 3, '2026-02-19 10:00:00', '2026-02-19 10:45:00', 'Scheduled', NULL),
    (62, 1, 1, 2, 8, '2026-02-19 11:30:00', '2026-02-19 12:00:00', 'Scheduled', NULL),
    (63, 1, 1, 3, 4, '2026-02-19 14:00:00', '2026-02-19 15:00:00', 'Scheduled', NULL),
    (64, 1, 1, 4, 10, '2026-02-19 15:30:00', '2026-02-19 16:00:00', 'Scheduled', NULL);

-- JUEVES 20 de Febrero 2026
INSERT INTO [Appointments] ([Id], [BusinessId], [ProviderId], [CustomerId], [ServiceId], [StartTime], [EndTime], [Status], [Notes])
VALUES 
    (65, 1, 1, 5, 9, '2026-02-20 09:00:00', '2026-02-20 11:00:00', 'Scheduled', 'Alisado con botox'),
    (66, 1, 1, 6, 1, '2026-02-20 11:30:00', '2026-02-20 12:00:00', 'Scheduled', NULL),
    (67, 1, 1, 7, 3, '2026-02-20 14:00:00', '2026-02-20 14:45:00', 'Scheduled', NULL),
    (68, 1, 1, 8, 5, '2026-02-20 15:00:00', '2026-02-20 15:20:00', 'Scheduled', NULL),
    (69, 1, 1, 9, 1, '2026-02-20 16:00:00', '2026-02-20 16:30:00', 'Scheduled', NULL);

-- VIERNES 21 de Febrero 2026
INSERT INTO [Appointments] ([Id], [BusinessId], [ProviderId], [CustomerId], [ServiceId], [StartTime], [EndTime], [Status], [Notes])
VALUES 
    (70, 1, 1, 10, 7, '2026-02-21 09:00:00', '2026-02-21 10:00:00', 'Scheduled', 'Boda el sábado'),
    (71, 1, 1, 11, 1, '2026-02-21 10:30:00', '2026-02-21 11:00:00', 'Scheduled', NULL),
    (72, 1, 1, 12, 4, '2026-02-21 11:30:00', '2026-02-21 12:30:00', 'Scheduled', NULL),
    (73, 1, 1, 13, 6, '2026-02-21 14:00:00', '2026-02-21 14:45:00', 'Scheduled', NULL),
    (74, 1, 1, 14, 8, '2026-02-21 15:00:00', '2026-02-21 15:30:00', 'Scheduled', NULL),
    (75, 1, 1, 15, 1, '2026-02-21 16:00:00', '2026-02-21 16:30:00', 'Scheduled', NULL);

-- SÁBADO 22 de Febrero 2026
INSERT INTO [Appointments] ([Id], [BusinessId], [ProviderId], [CustomerId], [ServiceId], [StartTime], [EndTime], [Status], [Notes])
VALUES 
    (76, 1, 1, 1, 2, '2026-02-22 09:00:00', '2026-02-22 10:30:00', 'Scheduled', 'Tonos cálidos'),
    (77, 1, 1, 2, 7, '2026-02-22 11:00:00', '2026-02-22 12:00:00', 'Scheduled', 'Invitada a boda'),
    (78, 1, 1, 3, 1, '2026-02-22 13:00:00', '2026-02-22 13:30:00', 'Scheduled', NULL),
    (79, 1, 1, 4, 3, '2026-02-22 14:00:00', '2026-02-22 14:45:00', 'Scheduled', NULL),
    (80, 1, 1, 5, 4, '2026-02-22 15:00:00', '2026-02-22 16:00:00', 'Scheduled', NULL),
    (81, 1, 1, 6, 10, '2026-02-22 16:30:00', '2026-02-22 17:00:00', 'Scheduled', NULL);

-- LUNES 24 de Febrero 2026
INSERT INTO [Appointments] ([Id], [BusinessId], [ProviderId], [CustomerId], [ServiceId], [StartTime], [EndTime], [Status], [Notes])
VALUES 
    (82, 1, 1, 7, 1, '2026-02-24 09:00:00', '2026-02-24 09:30:00', 'Scheduled', NULL),
    (83, 1, 1, 8, 5, '2026-02-24 10:00:00', '2026-02-24 10:20:00', 'Scheduled', NULL),
    (84, 1, 1, 9, 8, '2026-02-24 11:00:00', '2026-02-24 11:30:00', 'Scheduled', NULL),
    (85, 1, 1, 10, 3, '2026-02-24 14:00:00', '2026-02-24 14:45:00', 'Scheduled', NULL),
    (86, 1, 1, 11, 1, '2026-02-24 15:30:00', '2026-02-24 16:00:00', 'Scheduled', NULL);

-- MARTES 25 de Febrero 2026
INSERT INTO [Appointments] ([Id], [BusinessId], [ProviderId], [CustomerId], [ServiceId], [StartTime], [EndTime], [Status], [Notes])
VALUES 
    (87, 1, 1, 12, 6, '2026-02-25 09:00:00', '2026-02-25 09:45:00', 'Scheduled', 'Botox capilar'),
    (88, 1, 1, 13, 1, '2026-02-25 10:00:00', '2026-02-25 10:30:00', 'Scheduled', NULL),
    (89, 1, 1, 14, 4, '2026-02-25 11:00:00', '2026-02-25 12:00:00', 'Scheduled', NULL),
    (90, 1, 1, 15, 7, '2026-02-25 14:00:00', '2026-02-25 15:00:00', 'Scheduled', 'Entrevista trabajo'),
    (91, 1, 1, 1, 10, '2026-02-25 15:30:00', '2026-02-25 16:00:00', 'Scheduled', NULL);

-- MIÉRCOLES 26 de Febrero 2026
INSERT INTO [Appointments] ([Id], [BusinessId], [ProviderId], [CustomerId], [ServiceId], [StartTime], [EndTime], [Status], [Notes])
VALUES 
    (92, 1, 1, 2, 1, '2026-02-26 09:00:00', '2026-02-26 09:30:00', 'Scheduled', NULL),
    (93, 1, 1, 3, 3, '2026-02-26 10:00:00', '2026-02-26 10:45:00', 'Scheduled', NULL),
    (94, 1, 1, 4, 8, '2026-02-26 11:30:00', '2026-02-26 12:00:00', 'Scheduled', NULL),
    (95, 1, 1, 5, 5, '2026-02-26 14:00:00', '2026-02-26 14:20:00', 'Scheduled', NULL),
    (96, 1, 1, 6, 1, '2026-02-26 15:00:00', '2026-02-26 15:30:00', 'Scheduled', NULL);

-- JUEVES 27 de Febrero 2026
INSERT INTO [Appointments] ([Id], [BusinessId], [ProviderId], [CustomerId], [ServiceId], [StartTime], [EndTime], [Status], [Notes])
VALUES 
    (97, 1, 1, 7, 2, '2026-02-27 09:00:00', '2026-02-27 10:30:00', 'Scheduled', 'Platinado'),
    (98, 1, 1, 8, 1, '2026-02-27 11:00:00', '2026-02-27 11:30:00', 'Scheduled', NULL),
    (99, 1, 1, 9, 4, '2026-02-27 12:00:00', '2026-02-27 13:00:00', 'Scheduled', NULL),
    (100, 1, 1, 10, 6, '2026-02-27 14:00:00', '2026-02-27 14:45:00', 'Scheduled', NULL),
    (101, 1, 1, 11, 1, '2026-02-27 15:30:00', '2026-02-27 16:00:00', 'Scheduled', NULL);

-- VIERNES 28 de Febrero 2026
INSERT INTO [Appointments] ([Id], [BusinessId], [ProviderId], [CustomerId], [ServiceId], [StartTime], [EndTime], [Status], [Notes])
VALUES 
    (102, 1, 1, 12, 7, '2026-02-28 09:00:00', '2026-02-28 10:00:00', 'Scheduled', 'Fiesta de egresados'),
    (103, 1, 1, 13, 1, '2026-02-28 10:30:00', '2026-02-28 11:00:00', 'Scheduled', NULL),
    (104, 1, 1, 14, 3, '2026-02-28 11:30:00', '2026-02-28 12:15:00', 'Scheduled', NULL),
    (105, 1, 1, 15, 9, '2026-02-28 14:00:00', '2026-02-28 16:00:00', 'Scheduled', 'Alisado definitivo'),
    (106, 1, 1, 1, 8, '2026-02-28 16:30:00', '2026-02-28 17:00:00', 'Scheduled', NULL);

SET IDENTITY_INSERT [Appointments] OFF;
GO

-- =============================================
-- Resumen de datos insertados:
-- - 10 Servicios
-- - 15 Clientes
-- - 106 Turnos (del 6 al 28 de febrero de 2026)
-- =============================================

PRINT 'Datos de prueba insertados correctamente';
PRINT '- 10 Servicios';
PRINT '- 15 Clientes';
PRINT '- 106 Turnos (6-28 febrero 2026)';
PRINT 'Business ID: 1';
PRINT 'Provider ID: 1';
GO

