# Task Spec: Toi uu API Dang nhap (Login/Auth Hardening)

## 1) Muc tieu

Ban la AI implementation agent, can toi uu va harden luong dang nhap (`/api/Auth/login`) cua du an `QuanLyDoanKham` theo huong an toan, de bao tri, va han che regression.

Ket qua mong doi:
- Giam rui ro bao mat nghiem trong trong auth flow.
- Cai thien hieu nang query lien quan den login/refresh token.
- Chuan hoa kien truc auth de de mo rong va test.

## 2) Phan tich cau truc folder hien tai (ngu canh bat buoc)

### Top-level structure
- `QuanLyDoanKham.API`: Backend ASP.NET Core (auth API nam o day).
- `QuanLyDoanKham.Web`: Frontend Vue + Pinia (goi login/refresh/profile API).
- `Database`: Script/DB artifact.
- `Test_Reports`: Bao cao test.
- `todo`: Thu muc task spec (file nay).

### Backend auth scope
- `QuanLyDoanKham.API/Controllers/AuthController.cs`
  - Chua login, refresh-token, profile, register, change-password, reset password request, va endpoint khan cap reset admin.
- `QuanLyDoanKham.API/Program.cs`
  - Cau hinh middleware, JWT, CORS.
- `QuanLyDoanKham.API/DTOs/AuthDtos.cs`
  - DataAnnotations cho request/response DTO.
- `QuanLyDoanKham.API/Data/ApplicationDbContext.cs`
  - Truy cap bang Users/Roles/PasswordResetRequests.
- `QuanLyDoanKham.API/Models/Entities.cs`
  - Dinh nghia entity auth domain.

### Frontend auth scope
- `QuanLyDoanKham.Web/src/stores/auth.js`
  - Luu `token` + `refreshToken` trong localStorage/sessionStorage.
  - Tu refresh token khi 401.

## 3) Van de hien tai can giai quyet

1. **Critical**: Endpoint `POST /api/Auth/reset-admin-fix-emergency` khong co `[Authorize]`.
2. **High**: CORS dang `AllowAnyOrigin` trong `Program.cs`.
3. **High**: Chua co rate limiting / brute-force protection cho login.
4. **High**: Refresh token sinh bang `Guid` noi chuoi, chua dung CSPRNG.
5. **High**: Refresh token luu plaintext trong DB.
6. **High**: Secret (JWT key, ket noi DB, API key) dang co dau hieu nam trong config file.
7. **Medium**: Message login phan biet "khong ton tai tai khoan" vs "sai mat khau" -> de bi enum user.
8. **Medium**: Auth logic tap trung qua lon trong 1 controller, kho test va kho bao tri.
9. **Medium**: Chua co backend integration tests cho login/refresh/abuse case.

## 4) Pham vi cong viec (bat buoc)

### P0 - Security hotfix (uu tien cao nhat)
- Bao ve hoac vo hieu hoa endpoint khan cap reset admin.
  - Option uu tien: xoa endpoint neu khong con can.
  - Neu giu lai: bat buoc `[Authorize(Roles = "Admin")]` + feature flag/env guard + audit log.
- Chuan hoa message login fail thanh 1 dang chung (vd: "Thong tin dang nhap khong hop le").
- Bo sung rate limiting cho endpoint login (theo IP + login identifier).
- Tighten CORS: allowlist origin frontend thuc te, khong dung `AllowAnyOrigin`.
- Dua secrets sang environment variables / user-secrets, khong hardcode fallback key yeu.

### P1 - Token model hardening
- Tao refresh token bang CSPRNG (`RandomNumberGenerator`) + do dai du manh.
- Luu hash refresh token (khong luu plaintext).
- Implement refresh token rotation:
  - Moi lan refresh -> cap token moi, token cu het hieu luc.
  - Co co che detect token reuse (neu kha thi trong scope hien tai).
- Chinh policy het han:
  - Access token ngan han.
  - Refresh token co expiry ro rang + revoke khi logout/compromise.

### P2 - Kien truc + quality
- Tach auth business logic khoi `AuthController` vao service layer (`IAuthService`, `AuthService`).
- Gia dinh migration schema neu can them cot/chi muc cho token hash.
- Bo sung global exception handling cho auth path (tranh throw raw ra client).
- Bo sung backend tests:
  - login success/fail
  - refresh success/expired/reused
  - endpoint protection (nhat la emergency endpoint)
  - rate-limit behavior co ban

### Frontend alignment (neu doi contract)
- Neu doi sang cookie-based refresh: cap nhat `QuanLyDoanKham.Web/src/stores/auth.js` va API client.
- Dam bao khong vo luong auto-refresh loop.
- Validate lai luong login/logout/refresh tren UI.

## 5) File du kien can sua/tao

Bat buoc xem xet:
- `QuanLyDoanKham.API/Controllers/AuthController.cs`
- `QuanLyDoanKham.API/Program.cs`
- `QuanLyDoanKham.API/DTOs/AuthDtos.cs`
- `QuanLyDoanKham.API/Data/ApplicationDbContext.cs`
- `QuanLyDoanKham.API/Models/Entities.cs`

Co the tao moi:
- `QuanLyDoanKham.API/Services/Auth/IAuthService.cs`
- `QuanLyDoanKham.API/Services/Auth/AuthService.cs`
- File migration moi trong `QuanLyDoanKham.API/Migrations/`
- Test files trong backend test project (neu chua co, tao moi test project nhe, khong pha vo solution).

Canh bao:
- Khong pha vo API contract hien tai neu chua co ly do ro rang.
- Neu can doi contract response/request, phai cap nhat frontend tuong ung trong cung task.

## 6) Acceptance criteria (Done definition)

1. Login endpoint khong de lo enum account qua thong diep loi.
2. Endpoint emergency reset admin da duoc xu ly an toan (xoa hoac bao ve chat).
3. Login co rate limit hoat dong, duoc test it nhat 1 case.
4. Refresh token:
   - Sinh bang CSPRNG
   - Khong luu plaintext trong DB
   - Rotation hop le sau refresh
5. CORS duoc gioi han theo allowlist cau hinh.
6. Secret nhay cam khong con hardcode trong source chinh.
7. Auth flow chay pass cac test moi va khong lam hong test cu.
8. Co tai lieu release note ngan mo ta thay doi auth.

## 7) Test plan bat buoc

### Backend
- `POST /api/Auth/login`
  - Dung user/pass -> 200 + token payload hop le.
  - Sai thong tin -> 401 voi message chung.
  - Thu bruteforce nhanh -> bi gioi han theo policy.
- `POST /api/Auth/refresh-token`
  - Refresh token hop le -> cap token moi + refresh moi.
  - Refresh token het han/khong hop le -> 401.
  - Reuse token cu (neu da rotate) -> bi tu choi.
- Endpoint emergency
  - Anonymous khong truy cap duoc.

### Frontend smoke (neu co thay doi contract)
- Login thanh cong.
- Tu refresh token hoat dong dung.
- Logout xoa trang thai auth dung.

## 8) Rang buoc implementation

- Khong dung command pha huy (`git reset --hard`, xoa file hang loat).
- Khong commit secret.
- Uu tien thay doi nho, ro, co migration/tach service tung buoc de de review.
- Neu gap ambiguity ve contract frontend-backend, chon backward-compatible truoc.

## 9) Prompt giao cho AI implementation (copy dung khoi nay)

```text
Ban hay thuc hien task toi uu va harden Login API cho du an QuanLyDoanKham theo spec sau:

Muc tieu:
- Tang bao mat login/refresh flow.
- Giam rui ro brute-force, token theft, secret leakage.
- Nang cao kha nang bao tri bang cach tach auth service va them test.

Pham vi va uu tien:
P0:
1) Xu ly endpoint /api/Auth/reset-admin-fix-emergency an toan (uu tien xoa; neu giu lai thi Authorize + env-guard + audit).
2) Chuan hoa login fail message (khong enum account).
3) Them rate limit cho login.
4) Tighten CORS allowlist.
5) Dua secret sang env/user-secrets, bo fallback key yeu.

P1:
6) Refresh token sinh bang RandomNumberGenerator.
7) Chi luu hash refresh token trong DB.
8) Implement refresh token rotation + tu choi token reuse.

P2:
9) Tach auth logic khoi AuthController thanh AuthService.
10) Them global exception handling path cho auth.
11) Them backend integration tests cho login/refresh/rate-limit/protection.

File can tap trung:
- QuanLyDoanKham.API/Controllers/AuthController.cs
- QuanLyDoanKham.API/Program.cs
- QuanLyDoanKham.API/DTOs/AuthDtos.cs
- QuanLyDoanKham.API/Data/ApplicationDbContext.cs
- QuanLyDoanKham.API/Models/Entities.cs
- (co the tao) QuanLyDoanKham.API/Services/Auth/IAuthService.cs
- (co the tao) QuanLyDoanKham.API/Services/Auth/AuthService.cs
- migration files neu can

Acceptance criteria:
- Login fail tra message chung.
- Emergency endpoint an toan.
- Login co rate limit.
- Refresh token CSPRNG + hash-at-rest + rotation.
- CORS khong con AllowAnyOrigin.
- Secrets khong hardcode trong source.
- Test backend auth pass.

Yeu cau thuc thi:
- Sau moi thay doi lon, chay test/lint lien quan.
- Bao cao danh sach file da sua, ly do, va ket qua test.
- Neu can doi frontend contract auth, phai cap nhat frontend trong cung PR.
```

## 10) Output mong doi tu AI implementation

- Danh sach file da sua.
- Mo ta thay doi theo P0/P1/P2.
- Ket qua test command da chay.
- Danh sach risk con ton dong (neu co) + buoc tiep theo de hoan thien.

