const path = require("path");
const { chromium } = require("playwright");

const htmlPath = path.resolve(__dirname, "election-campaign-management-mockups.html");
const url = `file:///${htmlPath.replace(/\\/g, "/")}`;

async function shot(page, selector, fileName) {
  const element = await page.locator(selector).first();
  await element.screenshot({
    path: path.resolve(__dirname, fileName),
    animations: "disabled"
  });
}

(async () => {
  const browser = await chromium.launch({
    executablePath: "C:\\Program Files\\Google\\Chrome\\Application\\chrome.exe"
  });
  const page = await browser.newPage({ viewport: { width: 1500, height: 1200 }, deviceScaleFactor: 1 });
  await page.goto(url, { waitUntil: "networkidle" });
  await page.emulateMedia({ media: "screen" });

  await shot(page, "main.page:nth-of-type(1)", "public-homepage-desktop.png");
  await shot(page, "main.page:nth-of-type(2)", "admin-dashboard-desktop.png");
  await shot(page, "main.mobile-frame", "public-homepage-mobile.png");
  await shot(page, "main.admin-mobile-frame", "admin-dashboard-mobile.png");

  await browser.close();
})();
