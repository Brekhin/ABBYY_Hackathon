create database HackathonABBYY;
use HackathonABBYY;
drop table categories;
drop table costs;
drop table users;
create table users(
	user_id int primary key,
	name_user varchar(40)
);

create table costs(
	id int primary key IDENTITY(1,1) not null,
	user_id int not null,
	amount float,
	dt datetime,
	FOREIGN KEY (user_id) REFERENCES users(user_id)
);

create table categories(
	name_category varchar(40),
	cost_id int IDENTITY(1,1) not null, 
	FOREIGN KEY (cost_id) REFERENCES costs(id)
);
select * from users
select * from costs
select * from categories

insert into users values(1,'Misha');
insert into costs values(1, 1, 100, '1999-01-08 04:05:06')
insert into costs values(2, 1, 300, '1999-01-08 04:05:06')
insert into costs values(3, 1, 50, '1999-01-08 04:05:06')
insert into costs values(4, 1, 42, '1999-01-08 04:05:06')
insert into categories values('Транспорт', 1)
insert into categories values('Транспорт', 2)
insert into categories values('Мобильная связь', 3)
insert into categories values('Мобильная связь', 4)


select * from costs

select * from categories

select cat.name_category, sum(cos.amount)
from categories cat, costs cos
where cat.cost_id = cos.id
group by cat.name_category