import requests
import re
import uuid
import random
import string

base = "http://localhost:5253"
reg = base + "/Register/Register"
admin = base + "/AdminUsers/index"


def extract_csrf_token(html: str) -> str:
    m = re.search(r'name="__RequestVerificationToken"[^>]*value="([^"]*)"', html)
    if not m:
        m = re.search(r'__RequestVerificationToken[^>]*value="([^"]*)"', html)
    if not m:
        raise RuntimeError("❌ __RequestVerificationToken не найден в HTML")
    return m.group(1)


def gen_captcha_answer_2step():
    # UI: a+b then (a+b)+c
    a = random.randint(1, 9)
    b = random.randint(1, 9)
    c = random.randint(1, 9)
    step1 = a + b
    return step1 + c


def random_suffix(n=6):
    return "".join(random.choices(string.ascii_lowercase + string.digits, k=n))


def main():
    s = requests.Session()

    print("🔍 GET registration page...")
    r = s.get(reg, timeout=15)
    print("GET status:", r.status_code, "len:", len(r.text))

    if r.status_code != 200:
        print("\n❌ GET failed. Response snippet:")
        print(r.text[:2000])
        return

    token = extract_csrf_token(r.text)
    print("✅ token:", token[:25], "...")

    email = f"test_py_{uuid.uuid4().hex[:8]}_{random_suffix(4)}@local"
    password = "Admin12345"

    captcha_a = gen_captcha_answer_2step()

    payload = {
        "LastName": "Test",
        "FirstName": "User",
        "MiddleName": "",
        "Position": "HR Assistant",
        "Email": email,
        "Password": password,
        "Role": "HR",
        "__RequestVerificationToken": token,

        # Captcha: сервер проверяет CaptchaAnswer
        "CaptchaAnswer": str(captcha_a),
    }

    print("\n📤 POST registration...")
    print("   email:", email)
    print("   password:", password, "(len=", len(password), ")")
    print("   captchaAnswer:", captcha_a)

    r2 = s.post(reg, data=payload, allow_redirects=False, timeout=20)
    print("\n📊 POST result:")
    print("   status:", r2.status_code)
    print("   Location:", r2.headers.get("Location", "—"))
    print("   resp_len:", len(r2.text))

    if r2.status_code != 302:
        print("\n⚠️ Registration failed. Response snippet:")
        print(r2.text[:2000])
        return

    print("\n✅ Registration SUCCESS (302 redirect).")

    print("\n🌐 GET Admin page...")
    r3 = s.get(admin, timeout=15)
    print("Admin status:", r3.status_code, "len:", len(r3.text))

    if r3.status_code == 200:
        if email in r3.text:
            print("✅ Email found in AdminUsers UI:", email)
        else:
            print("❌ Email NOT found in AdminUsers UI.")
            print("Admin snippet:")
            print(r3.text[:1000])
    else:
        print("⚠️ Admin UI not accessible at:", admin)


if __name__ == "__main__":
    main()
