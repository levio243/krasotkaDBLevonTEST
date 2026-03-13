-- Удалить БД
-- drop database if exists saloonBeauty;

create database if not exists saloonBeauty;
use saloonBeauty;

-- создать таблицу servTypes
create table if not exists servTypes 
(
  servTypeCode int primary key,
  servType varchar(15),
  servTypeActivity varchar(5) default 'да'
);

create table if not exists masters 
(
 masterCode   int primary key,
 masterName varchar(20),
 masterTel varchar(12),
 servTypeCode int,
 masterActivity varchar(5) default 'да',
 foreign key (servTypeCode) references servTypes(servTypeCode)
);

create table if not exists clients 
(
  clientCode int primary key,
  clientName varchar(20),
  clientTel varchar(12),
  clientActivity varchar(5) default 'да'
);

-- создать таблицу Услуги
create table if not exists services
(
servCode int primary key,
servName varchar(300),
servPrice int,
servDuration int,
servTypeCode int,
servActivity varchar(5) default 'да',
 foreign key (servTypeCode) references servTypes(servTypeCode)
);
 
 -- создать таблицу Оказание услуг
create table if not exists appointments
(
    appCode      int primary key,
    masterCode   int,
    clientCode   int,
    servTypeCode int,
    servCode     int,
    queueFrom    int,
    queueTo      int,
    appDate      date,
    foreign key (masterCode)   references masters   (masterCode),
    foreign key (clientCode)   references clients   (clientCode),
    foreign key (servTypeCode) references servTypes (servTypeCode),
    foreign key (servCode)     references services  (servCode)
);